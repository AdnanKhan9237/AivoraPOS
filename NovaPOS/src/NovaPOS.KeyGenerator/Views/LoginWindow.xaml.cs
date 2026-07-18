using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaPOS.KeyGenerator.Data;
using NovaPOS.KeyGenerator.Services;
using NovaPOS.KeyGenerator.ViewModels;

namespace NovaPOS.KeyGenerator.Views;

public partial class LoginWindow : Window
{
    private readonly MasterPasswordService _masterPasswordService = new();

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void OnUnlockClick(object sender, RoutedEventArgs e)
    {
        var password = PasswordBox.Password;

        if (!_masterPasswordService.IsConfigured)
        {
            if (password.Length < 8)
            {
                ShowError("Set a master password with at least 8 characters.");
                return;
            }

            _masterPasswordService.SetMasterPassword(password);
        }
        else if (!_masterPasswordService.Verify(password))
        {
            ShowError("Incorrect master password.");
            return;
        }

        var services = new ServiceCollection();
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NovaPOS",
            "KeyGenerator",
            "keygen.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        services.AddDbContextFactory<KeyGeneratorDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));
        services.AddSingleton<KeyGenerationService>();
        services.AddSingleton<MainViewModel>();

        var provider = services.BuildServiceProvider();

        using var db = provider.GetRequiredService<IDbContextFactory<KeyGeneratorDbContext>>().CreateDbContext();
        db.Database.EnsureCreated();

        var mainWindow = new MainWindow
        {
            DataContext = provider.GetRequiredService<MainViewModel>()
        };

        mainWindow.Show();
        Close();
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
