using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using NovaPOS.Core;
using NovaPOS.Core.Constants;
using NovaPOS.Core.Interfaces.Services;

namespace NovaPOS.App.Services;

public sealed class CrashReportingService : ICrashReportingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly string _preferenceFilePath;

    public CrashReportingService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
        _preferenceFilePath = Path.Combine(AppPaths.AppDataRoot, "crash-reporting.json");
        Preference = LoadPreference();
    }

    public CrashReportPreference Preference { get; private set; }

    public async Task<bool> PromptAndReportAsync(Exception exception, string source, CancellationToken cancellationToken = default)
    {
        if (Preference == CrashReportPreference.Never)
        {
            return false;
        }

        if (Preference == CrashReportPreference.Allow)
        {
            await ReportAsync(exception, source, cancellationToken);
            return true;
        }

        var result = MessageBox.Show(
            "Send an anonymous crash report to the developer?\n\nNo business data or personal information is included.",
            "AivoraPOS Crash Report",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        switch (result)
        {
            case MessageBoxResult.Yes:
                SetPreference(CrashReportPreference.Allow);
                await ReportAsync(exception, source, cancellationToken);
                return true;
            case MessageBoxResult.No:
                return false;
            case MessageBoxResult.Cancel:
                SetPreference(CrashReportPreference.Never);
                return false;
            default:
                return false;
        }
    }

    public async Task ReportAsync(Exception exception, string source, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                app = ProductInfo.AppName,
                version = AppVersion.Current,
                source,
                message = exception.Message,
                exceptionType = exception.GetType().FullName,
                stackTrace = exception.StackTrace,
                os = RuntimeInformation.OSDescription,
                framework = RuntimeInformation.FrameworkDescription,
                machine = Environment.Is64BitOperatingSystem ? "x64" : "x86",
                timestampUtc = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(ProductInfo.CrashReportUrl, content, cancellationToken);
        }
        catch (HttpRequestException)
        {
            // Silent failure for offline scenarios.
        }
        catch (TaskCanceledException)
        {
            // Silent failure for offline scenarios.
        }
    }

    public void SetPreference(CrashReportPreference preference)
    {
        Preference = preference;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_preferenceFilePath)!);
            var json = JsonSerializer.Serialize(new CrashReportingPreferenceDto { Preference = preference.ToString() }, JsonOptions);
            File.WriteAllText(_preferenceFilePath, json);
        }
        catch (IOException)
        {
            // Best effort persistence.
        }
    }

    private CrashReportPreference LoadPreference()
    {
        try
        {
            if (!File.Exists(_preferenceFilePath))
            {
                return CrashReportPreference.Ask;
            }

            var json = File.ReadAllText(_preferenceFilePath);
            var dto = JsonSerializer.Deserialize<CrashReportingPreferenceDto>(json, JsonOptions);
            return Enum.TryParse<CrashReportPreference>(dto?.Preference, ignoreCase: true, out var preference)
                ? preference
                : CrashReportPreference.Ask;
        }
        catch (IOException)
        {
            return CrashReportPreference.Ask;
        }
        catch (JsonException)
        {
            return CrashReportPreference.Ask;
        }
    }

    private sealed class CrashReportingPreferenceDto
    {
        public string Preference { get; set; } = CrashReportPreference.Ask.ToString();
    }
}
