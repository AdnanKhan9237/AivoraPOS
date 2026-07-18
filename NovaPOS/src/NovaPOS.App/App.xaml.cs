using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NovaPOS.App.Logging;
using NovaPOS.App.ViewModels;
using NovaPOS.App.ViewModels.Reports;
using NovaPOS.App.ViewModels.Sales;
using NovaPOS.App.Views;
using NovaPOS.Core.Constants;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Licensing;
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
                else if (integrityResult.IsRunningInVirtualMachine)
                {
                    Log.Warning("Application is running inside a virtual machine.");
                }
            }

            using (var initScope = CreateScope())
            {
                var initializer = initScope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
                await initializer.InitializeAsync();
            }

            using (var licenseScope = CreateScope())
            {
                var licenseService = licenseScope.ServiceProvider.GetRequiredService<ILicenseService>();
                var licenseResult = await licenseService.ValidateOnLaunchAsync();

                if (!await HandleLicenseGateAsync(licenseService, licenseResult))
                {
                    Shutdown(0);
                    return;
                }
            }

            var sessionCoordinator = _host.Services.GetRequiredService<ApplicationSessionCoordinator>();
            sessionCoordinator.LockScreenRequested += OnLockScreenRequested;

            using (var sessionLicenseScope = CreateScope())
            {
                var licenseService = sessionLicenseScope.ServiceProvider.GetRequiredService<ILicenseService>();
                await RunApplicationSessionAsync(licenseService);
            }
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

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddNovaPOSSecurity();
        services.AddNovaPOSData();
        services.AddNovaPOSLicensing();
        services.AddNovaPOSReporting();

        services.AddSingleton<ApplicationSessionCoordinator>();
        services.AddSingleton<MainWindow>();
        services.AddScoped<SalesViewModel>();
        services.AddScoped<ReportsViewModel>();
        services.AddScoped<MainViewModel>(sp =>
        {
            var coordinator = sp.GetRequiredService<ApplicationSessionCoordinator>();
            return new MainViewModel(
                sp.GetRequiredService<ILicenseService>(),
                sp.GetRequiredService<ICurrentUserService>(),
                sp.GetRequiredService<IAuthorizationService>(),
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<SalesViewModel>(),
                vm =>
                {
                    var window = new AuditLogWindow { DataContext = vm, Owner = Current.MainWindow };
                    window.ShowDialog();
                },
                () => coordinator.RequestLockScreen());
        });
    }

    private IServiceScope CreateScope() => _host!.Services.CreateScope();

    private async Task RunApplicationSessionAsync(ILicenseService licenseService)
    {
        var sessionTimeout = _host!.Services.GetRequiredService<ISessionTimeoutService>();
        sessionTimeout.SessionTimedOut += OnSessionTimedOut;

        while (true)
        {
            var user = await ShowLockScreenAsync();
            if (user is null)
            {
                Shutdown(0);
                return;
            }

            _mainWindow ??= _host.Services.GetRequiredService<MainWindow>();
            _sessionScope?.Dispose();
            _sessionScope = CreateScope();
            var mainViewModel = _sessionScope.ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel.RefreshLicenseStatus();
            mainViewModel.RefreshUserStatus();
            mainViewModel.OnActivateLicenseRequested += OnActivateLicenseRequested;

            _mainWindow.DataContext = mainViewModel;
            _mainWindow.Show();
            sessionTimeout.Start();

            var sessionEnded = await WaitForMainWindowCloseAsync();
            sessionTimeout.Stop();
            mainViewModel.OnActivateLicenseRequested -= OnActivateLicenseRequested;
            _sessionScope.Dispose();
            _sessionScope = null;

            if (!sessionEnded)
            {
                Shutdown(0);
                return;
            }
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

    private void OnLockScreenRequested()
    {
        Dispatcher.Invoke(() => _mainWindow?.Hide());
    }

    private void OnSessionTimedOut(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            MessageBox.Show(
                "Your session was locked due to inactivity.",
                "Session Locked",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            _mainWindow?.Hide();
        });
    }

    private Task<User?> ShowLockScreenAsync()
    {
        var tcs = new TaskCompletionSource<User?>();
        var loginCompleted = false;

        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var window = new LockScreenWindow { Owner = _mainWindow };
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
            _mainWindow.IsVisibleChanged -= OnVisibilityChanged;
            tcs.TrySetResult(false);
        }

        void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_mainWindow is { IsVisible: false, IsLoaded: true })
            {
                tcs.TrySetResult(true);
            }
        }

        _mainWindow.Closed += OnClosed;
        _mainWindow.IsVisibleChanged += OnVisibilityChanged;

        return tcs.Task;
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

    private sealed class SensitiveLogEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            SensitiveDataDestructuringPolicy.EnrichSensitiveProperties(logEvent);
        }
    }
}
