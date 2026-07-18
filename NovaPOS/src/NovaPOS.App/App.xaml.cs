using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NovaPOS.App.ViewModels;
using NovaPOS.Core.Constants;
using NovaPOS.Core.Interfaces.Services;
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

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
            mainWindow.Show();

            Log.Information("NovaPOS started successfully.");
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

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
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
