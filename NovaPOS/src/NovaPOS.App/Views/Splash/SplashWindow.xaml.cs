using System.Windows;
using System.Windows.Media.Animation;
using NovaPOS.Core.Constants;

namespace NovaPOS.App.Views.Splash;

public partial class SplashWindow : Window
{
    private readonly TaskCompletionSource<bool> _closed = new();

    public SplashWindow()
    {
        InitializeComponent();
        CopyrightText.Text = ProductInfo.CopyrightShort;
        Loaded += OnLoaded;
    }

    public Task WaitForCloseAsync() => _closed.Task;

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(2500);

        var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
        fade.Completed += (_, _) =>
        {
            Close();
            _closed.TrySetResult(true);
        };

        BeginAnimation(OpacityProperty, fade);
    }
}
