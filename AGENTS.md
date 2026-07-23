# AGENTS.md

## Cursor Cloud specific instructions

### What this repo is
AivoraPOS is a **Windows-only WPF desktop Point-of-Sale** app (.NET 8, C# 12). The
solution lives under `NovaPOS/` (`NovaPOS/AivoraPOS.sln`). The SDK is pinned by
`NovaPOS/global.json` to `8.0.400` (`rollForward: latestFeature`).

Layout: `src/AivoraPOS.App` (WPF, `net8.0-windows`), `src/AivoraPOS.KeyGenerator`
(WPF vendor tool, `net8.0-windows`), and libraries `Core/Data/Security/Licensing/Reporting`
(`net8.0`). Test projects under `tests/` target `net8.0` (xUnit).

### Environment
- `.NET SDK 8.0.400` is installed at `~/.dotnet` (the update script re-installs it if missing).
  `~/.bashrc` exports `DOTNET_ROOT` and adds `~/.dotnet` to `PATH`. New shells pick this up;
  if `dotnet` is not found in a non-login shell, run `export PATH="$HOME/.dotnet:$PATH"`.
- `Directory.Build.props` sets `EnableWindowsTargeting=true`, so the **whole solution builds on
  Linux** (including the WPF projects, via a `wpftmp` cross-compile). Build warnings `NU1701`
  (WPF-only NuGet packages restored against .NET Framework) and `CA1416` (Windows-only WMI
  calls in `AivoraPOS.Security`) are expected and harmless on Linux.

### Build / test / run (all from `NovaPOS/`)
- Restore: `dotnet restore AivoraPOS.sln`
- Build: `dotnet build AivoraPOS.sln -c Debug`
- Test: `dotnet test AivoraPOS.sln -c Debug` (39 tests across 5 projects; all `net8.0`, run fine on Linux)
- There is **no lint/format gate**: no `.editorconfig` exists and CI (`.github/workflows/release.yml`,
  tag-triggered, `windows-latest`) only runs restore → build → test → publish. `dotnet format --verify-no-changes`
  reports pre-existing whitespace diffs against default rules; do not treat it as an enforced gate.

### Running the application — important caveat
- **The WPF GUI (`AivoraPOS.App`, `AivoraPOS.KeyGenerator`) cannot run on Linux/cloud VMs.** They are
  `net8.0-windows` WinExe apps and require Windows + the Desktop Runtime + a display. They build here but
  will not launch. Run the GUI on a Windows host with `dotnet run --project src/AivoraPOS.App/AivoraPOS.App.csproj`.
- To exercise the **core POS engine headlessly on Linux**, reference the `net8.0` libraries
  (`Core`, `Data`, `Security`, `Reporting`) from a small console app and drive them via DI:
  `services.AddAivoraPOSSecurity(); services.AddAivoraPOSData(); services.AddAivoraPOSReporting();`
  then `IDatabaseInitializer.InitializeAsync()` (migrate + seed), `IAuthService.LoginWithPasswordAsync`,
  `ICategoryService`/`IProductService`, `ISaleService.CompleteSaleAsync`, `IReceiptService`.
  This path works end-to-end on Linux (encrypted SQLite + QuestPDF receipt generation).

### Data / runtime notes
- Storage is embedded **encrypted SQLite** (SQLite3MC); no external DB server. Files live under
  `Environment.SpecialFolder.ApplicationData`/`AivoraPOS/` — on Linux that is `~/.config/AivoraPOS/`
  (`data/aivorapos.db`, `receipts/`, `reports/`, `logs/`).
- The DB password is **machine-bound** (`AesEncryptionService` fingerprint). A DB created on one machine
  won't open on another. To reset local state, delete `~/.config/AivoraPOS/data/aivorapos.db`.
- The app auto-migrates and seeds demo data + default users on first run. Seeded logins
  (`SeedingDefaults.cs`): admin `admin` / password `Admin@1234` / PIN `9073`; cashier `cashier` /
  `Cashier@1234` / PIN `2468`. Password login is Admin/Manager only; cashiers must use PIN.
