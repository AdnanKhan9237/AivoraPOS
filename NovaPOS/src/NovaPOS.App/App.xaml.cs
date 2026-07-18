using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NovaPOS.App.Logging;
using NovaPOS.App.Services;
using NovaPOS.App.ViewModels;
using NovaPOS.App.ViewModels.Login;
using NovaPOS.App.ViewModels.Products;
using NovaPOS.App.ViewModels.Reports;
using NovaPOS.App.ViewModels.Sales;
using NovaPOS.App.ViewModels.Settings;
using NovaPOS.App.ViewModels.Shell;
using NovaPOS.App.ViewModels.Users;
using NovaPOS.App.Views;
using NovaPOS.App.Views.Login;
using NovaPOS.Core.Constants;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Licensing;
using NovaPOS.Core.Interfaces.Navigation;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models;
using NovaPOS.Data.Extensions;
using NovaPOS.Licensing.Extensions;
using NovaPOS.Reporting.Extensions;
using NovaPOS.Security.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace NovaPOS.App;

public partial class App : Application
{
    private IHost? _host;
    private MainWindow? _mainWindow;
    private IServiceScope? _sessionScope;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        AppPaths.EnsureDirectoriesExist();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.With(new SensitiveLogEnricher())
            .Filter.ByExcluding(logEvent =>
            {
                var message = logEvent.RenderMessage();
                return message.Contains("password", StringComparison.OrdinalIgnoreCase)
                       || message.Contains(" pin ", StringComparison.OrdinalIgnoreCase)
                       || message.Contains("card number", StringComparison.OrdinalIgnoreCase);
            })
            .WriteTo.File(
                Path.Combine(AppPaths.LogsDirectory, "novapos-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .WriteTo.Debug()
            .CreateLogger();

        try
        {
            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices(ConfigureServices)
                .Build();

            await _host.StartAsync();

            using (var integrityScope = CreateScope())
            {
                var integrityService = integrityScope.ServiceProvider.GetRequiredService<IAppIntegrityService>();
                var integrityResult = await integrityService.VerifyAsync();
                if (!integrityResult.IsValid)
                {
                    MessageBox.Show(
                        integrityResult.Message,
                        "Integrity Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }

            // 1–2. Database migrations and seeding
            using (var initScope = CreateScope())
            {
                var initializer = initScope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
                await initializer.InitializeAsync();

                var themeService = initScope.ServiceProvider.GetRequiredService<IThemeService>();
                await themeService.InitializeAsync();
                await ConfigureSessionTimeoutAsync(initScope.ServiceProvider);
            }

            // 3. License gate
            LicenseCheckResult licenseResult;
            using (var licenseScope = CreateScope())
            {
                var licenseService = licenseScope.ServiceProvider.GetRequiredService<ILicenseService>();
                licenseResult = await licenseService.ValidateOnLaunchAsync();

                if (!await HandleLicenseGateAsync(licenseService, licenseResult))
                {
                    Shutdown(0);
                    return;
                }
            }

            // 4. Trial notice
            if (licenseResult.IsTrial)
            {
                await ShowTrialNoticeAsync(licenseResult);
            }

            // 5. Check for updates (non-blocking, fails silently offline)
            var updateCoordinator = _host.Services.GetRequiredService<UpdateCoordinator>();
            await updateCoordinator.CheckAsync();

            // 6–7. Login then main window
            await RunApplicationSessionAsync(updateCoordinator);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "NovaPOS failed to start.");
            ShowFriendlyError("NovaPOS could not start. Please check the log files and try again.");
            Shutdown(-1);
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddNovaPOSSecurity();
        services.AddNovaPOSData();
        services.AddNovaPOSLicensing();
        services.AddNovaPOSReporting();

        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<INavigationService, NavigationService>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<ICrashReportingService, CrashReportingService>();
        services.AddSingleton<UpdateCoordinator>();

        services.AddSingleton<ApplicationSessionCoordinator>();
        services.AddSingleton<MainWindow>();

        services.AddScoped<SalesViewModel>();
        services.AddScoped<ProductsViewModel>();
        services.AddScoped<ReportsViewModel>();
        services.AddScoped<AuditLogViewModel>();
        services.AddScoped<SettingsViewModel>();
        services.AddScoped<UsersViewModel>();
        services.AddScoped<LockOverlayViewModel>();
        services.AddScoped<MainViewModel>();
    }

    private IServiceScope CreateScope() => _host!.Services.CreateScope();

    private async Task RunApplicationSessionAsync(UpdateCoordinator? updateCoordinator)
    {
        var user = await ShowLoginWindowAsync();
        if (user is null)
        {
            Shutdown(0);
            return;
        }

        _mainWindow = _host!.Services.GetRequiredService<MainWindow>();
        _sessionScope = CreateScope();

        var sessionTimeout = _sessionScope.ServiceProvider.GetRequiredService<ISessionTimeoutService>();
        sessionTimeout.SessionTimedOut += OnSessionTimedOut;

        var mainViewModel = _sessionScope.ServiceProvider.GetRequiredService<MainViewModel>();
        mainViewModel.ApplyUpdateState(updateCoordinator);
        mainViewModel.OnActivateLicenseRequested += OnActivateLicenseRequested;
        mainViewModel.RefreshLicenseStatus();
        mainViewModel.RefreshUserStatus();
        await mainViewModel.RefreshInventoryAlertsAsync();

        _mainWindow.DataContext = mainViewModel;
        _mainWindow.Show();
        sessionTimeout.Start();

        var closed = await WaitForMainWindowCloseAsync();
        sessionTimeout.Stop();
        sessionTimeout.SessionTimedOut -= OnSessionTimedOut;
        mainViewModel.OnActivateLicenseRequested -= OnActivateLicenseRequested;
        mainViewModel.Dispose();

        _sessionScope.Dispose();
        _sessionScope = null;

        if (!closed)
        {
            Shutdown(0);
        }
    }

    private void OnActivateLicenseRequested()
    {
        if (_host is null || _sessionScope is null)
        {
            return;
        }

        var licenseService = _sessionScope.ServiceProvider.GetRequiredService<ILicenseService>();
        var mainViewModel = _sessionScope.ServiceProvider.GetRequiredService<MainViewModel>();
        _ = ShowActivationDialogAsync(licenseService, mainViewModel);
    }

    private void OnSessionTimedOut(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (_sessionScope?.ServiceProvider.GetService<MainViewModel>() is MainViewModel mainViewModel)
            {
                mainViewModel.RequestLock(DateTime.Now);
            }
        });
    }

    private Task<User?> ShowLoginWindowAsync()
    {
        var tcs = new TaskCompletionSource<User?>();
        var loginCompleted = false;

        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var window = new LoginWindow();
        window.DataContext = new LockScreenViewModel(
            authService,
            userRepository,
            user =>
            {
                loginCompleted = true;
                tcs.TrySetResult(user);
                window.Close();
            });

        window.Closed += (_, _) =>
        {
            if (!loginCompleted)
            {
                tcs.TrySetResult(null);
            }
        };

        window.ShowDialog();
        return tcs.Task;
    }

    private Task ShowTrialNoticeAsync(LicenseCheckResult licenseResult)
    {
        var tcs = new TaskCompletionSource();
        var window = new TrialRemainingNoticeWindow();
        window.DataContext = new TrialNoticeViewModel(
            licenseResult.Message,
            () =>
            {
                tcs.TrySetResult();
                window.Close();
            });

        window.ShowDialog();
        return tcs.Task;
    }

    private Task<bool> WaitForMainWindowCloseAsync()
    {
        if (_mainWindow is null)
        {
            return Task.FromResult(false);
        }

        var tcs = new TaskCompletionSource<bool>();

        void OnClosed(object? sender, EventArgs e)
        {
            _mainWindow!.Closed -= OnClosed;
            tcs.TrySetResult(false);
        }

        _mainWindow.Closed += OnClosed;
        return tcs.Task;
    }

    private async Task<bool> HandleLicenseGateAsync(ILicenseService licenseService, LicenseCheckResult licenseResult)
    {
        switch (licenseResult.Status)
        {
            case LicenseStatus.Invalid:
            {
                var activate = await ShowGateDialogAsync<InvalidLicenseWindow>(licenseResult.Message);
                return activate && await ShowActivationDialogAsync(licenseService, null);
            }

            case LicenseStatus.Expired when licenseResult.IsReadOnlyMode:
            {
                var activate = await ShowGateDialogAsync<LicenseExpiredWindow>(licenseResult.Message);
                if (!activate)
                {
                    return true;
                }

                return await ShowActivationDialogAsync(licenseService, null);
            }

            default:
                return licenseResult.CanRunApplication;
        }
    }

    private Task<bool> ShowGateDialogAsync<TWindow>(string message)
        where TWindow : Window, new()
    {
        var tcs = new TaskCompletionSource<bool>();
        var window = new TWindow();

        window.DataContext = new LicenseGateViewModel(message, activate =>
        {
            tcs.TrySetResult(activate);
            window.Close();
        });

        window.ShowDialog();
        return tcs.Task;
    }

    private async Task<bool> ShowActivationDialogAsync(ILicenseService licenseService, MainViewModel? mainViewModel)
    {
        var tcs = new TaskCompletionSource<bool>();
        var window = new ActivateLicenseWindow();

        window.DataContext = new ActivateLicenseViewModel(licenseService, success =>
        {
            tcs.TrySetResult(success);
            window.Close();
        });

        window.ShowDialog();
        var activated = await tcs.Task;

        if (activated)
        {
            await licenseService.ValidateOnLaunchAsync();
            mainViewModel?.RefreshLicenseStatus();
        }

        return activated;
    }

    private static async Task ConfigureSessionTimeoutAsync(IServiceProvider services)
    {
        var appSettingRepository = services.GetRequiredService<IAppSettingRepository>();
        var settingsService = services.GetRequiredService<ISettingsService>();
        var sessionTimeout = services.GetRequiredService<ISessionTimeoutService>();
        await settingsService.InvalidateCacheAsync();
        await settingsService.GetPosBehaviorAsync();
        var minutes = settingsService.Get(SettingKeys.IdleTimeoutMinutes, 5);
        if (minutes > 0)
        {
            sessionTimeout.IdleTimeout = TimeSpan.FromMinutes(minutes);
        }
        else
        {
            sessionTimeout.IdleTimeout = TimeSpan.FromDays(365);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _sessionScope?.Dispose();

        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        await Log.CloseAndFlushAsync();
        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled UI exception.");
        _ = ReportCrashAsync(e.Exception, "UI");
        ShowFriendlyError("Something went wrong. The action could not be completed, but NovaPOS will keep running.");
        e.Handled = true;
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "Unhandled domain exception.");
            _ = ReportCrashAsync(ex, "Domain");
            Dispatcher.Invoke(() => ShowFriendlyError("Something went wrong. NovaPOS encountered a critical error."));
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception.");
        _ = ReportCrashAsync(e.Exception, "Task");
        Dispatcher.Invoke(() => ShowFriendlyError("Something went wrong while completing a background task."));
        e.SetObserved();
    }

    private async Task ReportCrashAsync(Exception exception, string source)
    {
        if (_host is null)
        {
            return;
        }

        try
        {
            using var scope = _host.Services.CreateScope();
            var crashReporting = scope.ServiceProvider.GetRequiredService<ICrashReportingService>();
            await crashReporting.PromptAndReportAsync(exception, source);
        }
        catch (Exception reportEx)
        {
            Log.Warning(reportEx, "Crash reporting failed.");
        }
    }

    private static void ShowFriendlyError(string message)
    {
        MessageBox.Show(
            message,
            "NovaPOS",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private sealed class SensitiveLogEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            SensitiveDataDestructuringPolicy.EnrichSensitiveProperties(logEvent);
        }
    }
}
