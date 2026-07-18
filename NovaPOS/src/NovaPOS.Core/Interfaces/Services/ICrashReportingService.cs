namespace NovaPOS.Core.Interfaces.Services;

public enum CrashReportPreference
{
    Ask,
    Allow,
    Never
}

public interface ICrashReportingService
{
    CrashReportPreference Preference { get; }

    Task<bool> PromptAndReportAsync(Exception exception, string source, CancellationToken cancellationToken = default);

    Task ReportAsync(Exception exception, string source, CancellationToken cancellationToken = default);

    void SetPreference(CrashReportPreference preference);
}
