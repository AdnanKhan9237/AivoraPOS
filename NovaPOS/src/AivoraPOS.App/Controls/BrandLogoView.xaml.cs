using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AivoraPOS.App.Controls;

public partial class BrandLogoView : UserControl
{
    public static readonly DependencyProperty FallbackTextProperty =
        DependencyProperty.Register(nameof(FallbackText), typeof(string), typeof(BrandLogoView), new PropertyMetadata("AivoraPOS"));

    public static readonly DependencyProperty LogoHeightProperty =
        DependencyProperty.Register(nameof(LogoHeight), typeof(double), typeof(BrandLogoView), new PropertyMetadata(28.0, OnSizeChanged));

    public static readonly DependencyProperty LogoWidthProperty =
        DependencyProperty.Register(nameof(LogoWidth), typeof(double), typeof(BrandLogoView), new PropertyMetadata(double.NaN, OnSizeChanged));

    public static readonly DependencyProperty FallbackForegroundProperty =
        DependencyProperty.Register(nameof(FallbackForeground), typeof(System.Windows.Media.Brush), typeof(BrandLogoView));

    public static readonly DependencyProperty LogoSourceProperty =
        DependencyProperty.Register(nameof(LogoSource), typeof(string), typeof(BrandLogoView), new PropertyMetadata("full", OnLogoSourceChanged));

    public BrandLogoView()
    {
        InitializeComponent();
        Loaded += (_, _) => LoadLogo();
    }

    public string FallbackText
    {
        get => (string)GetValue(FallbackTextProperty);
        set => SetValue(FallbackTextProperty, value);
    }

    public double LogoHeight
    {
        get => (double)GetValue(LogoHeightProperty);
        set => SetValue(LogoHeightProperty, value);
    }

    public double LogoWidth
    {
        get => (double)GetValue(LogoWidthProperty);
        set => SetValue(LogoWidthProperty, value);
    }

    public System.Windows.Media.Brush? FallbackForeground
    {
        get => (System.Windows.Media.Brush?)GetValue(FallbackForegroundProperty);
        set => SetValue(FallbackForegroundProperty, value);
    }

    /// <summary>
    /// "full" loads Assets/logo.png; "icon" loads Assets/logo-icon.png.
    /// </summary>
    public string LogoSource
    {
        get => (string)GetValue(LogoSourceProperty);
        set => SetValue(LogoSourceProperty, value);
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BrandLogoView view)
        {
            view.ApplySize();
        }
    }

    private static void OnLogoSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BrandLogoView view && view.IsLoaded)
        {
            view.LoadLogo();
        }
    }

    private void ApplySize()
    {
        if (!double.IsNaN(LogoHeight))
        {
            Height = LogoHeight;
        }

        Width = double.IsNaN(LogoWidth) ? double.NaN : LogoWidth;
        if (LogoImage is not null)
        {
            if (!double.IsNaN(LogoHeight))
            {
                LogoImage.Height = LogoHeight;
            }

            if (!double.IsNaN(LogoWidth))
            {
                LogoImage.Width = LogoWidth;
            }
        }
    }

    private void LoadLogo()
    {
        FallbackTextBlock.Text = FallbackText;
        if (FallbackForeground is not null)
        {
            FallbackTextBlock.Foreground = FallbackForeground;
        }

        ApplySize();

        var fileName = string.Equals(LogoSource, "icon", StringComparison.OrdinalIgnoreCase)
            ? "logo-icon.png"
            : "logo.png";

        var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);
        if (!File.Exists(logoPath))
        {
            logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.png");
        }

        if (!File.Exists(logoPath))
        {
            ShowFallback();
            return;
        }

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(logoPath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            LogoImage.Source = bitmap;
            LogoImage.Visibility = Visibility.Visible;
            FallbackTextBlock.Visibility = Visibility.Collapsed;
        }
        catch
        {
            ShowFallback();
        }
    }

    private void OnImageFailed(object sender, ExceptionRoutedEventArgs e) => ShowFallback();

    private void ShowFallback()
    {
        LogoImage.Visibility = Visibility.Collapsed;
        FallbackTextBlock.Visibility = Visibility.Visible;
    }
}
