using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NovaPOS.App.ViewModels;
using NovaPOS.App.Views;
using NovaPOS.Core.Constants;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Licensing;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models;
using NovaPOS.Data.Extensions;
using NovaPOS.Licensing.Extensions;
using NovaPOS.Reporting.Extensions;
using NovaPOS.Security.Extensions;
using Serilog;

namespace NovaPOS.App;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        AppPaths.EnsureDirectoriesExist();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
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
                .ConfigureServices(services =>
                {
                    services.AddNovaPOSSecurity();
                    services.AddNovaPOSData();
                    services.AddNovaPOSLicensing();
                    services.AddNovaPOSReporting();
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            await _host.StartAsync();

            var initializer = _host.Services.GetRequiredService<IDatabaseInitializer>();
            await initializer.InitializeAsync();

            var licenseService = _host.Services.GetRequiredService<ILicenseService>();
            var licenseResult = await licenseService.ValidateOnLaunchAsync();

            if (!await HandleLicenseGateAsync(licenseService, licenseResult))
            {
                Shutdown(0);
                return;
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
            mainViewModel.RefreshLicenseStatus();
            mainViewModel.OnActivateLicenseRequested += () => _ = ShowActivationDialogAsync(licenseService, mainViewModel);
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();

            Log.Information("NovaPOS started successfully. License status: {Status}", licenseResult.Status);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "NovaPOS failed to start.");
            MessageBox.Show(
                "NovaPOS could not start. Please check the log files and try again.",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private async Task<bool> HandleLicenseGateAsync(ILicenseService licenseService, LicenseCheckResult licenseResult)
    {
        switch (licenseResult.Status)
        {
            case LicenseStatus.Invalid:
            {
                var activate = await ShowGateDialogAsync<InvalidLicenseWindow>(licenseResult.Message, showReadOnly: false);
                return activate && await ShowActivationDialogAsync(licenseService, null);
            }

            case LicenseStatus.Expired when licenseResult.IsReadOnlyMode:
            {
                var activate = await ShowGateDialogAsync<LicenseExpiredWindow>(licenseResult.Message, showReadOnly: true);
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

    private Task<bool> ShowGateDialogAsync<TWindow>(string message, bool showReadOnly)
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

    protected override async void OnExit(ExitEventArgs e)
    {
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
        MessageBox.Show(
            "An unexpected error occurred. The action could not be completed.",
            "NovaPOS",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
        e.Handled = true;
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "Unhandled domain exception.");
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception.");
        e.SetObserved();
    }
}
