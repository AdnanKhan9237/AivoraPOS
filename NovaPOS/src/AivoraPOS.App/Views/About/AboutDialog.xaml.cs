using System.Reflection;
using System.Windows;
using AivoraPOS.Core.Constants;

namespace AivoraPOS.App.Views.About;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
        DataContext = new AboutDialogViewModel();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}

public sealed class AboutDialogViewModel
{
    public string VersionText { get; } =
        $"Version {Assembly.GetExecutingAssembly().GetName().Version}";

    public string SupportEmail { get; } = ProductInfo.SupportEmail;

    public string CopyrightLine { get; } = ProductInfo.CopyrightShort;
}
