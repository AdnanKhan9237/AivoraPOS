using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Models.Settings;

public sealed class ReceiptSettingsDto
{
    public string HeaderText { get; set; } = string.Empty;
    public string FooterText { get; set; } = "Thank you for your business!";
    public bool ShowLogo { get; set; }
    public bool AutoPrint { get; set; }
    public string PrinterName { get; set; } = string.Empty;
    public ReceiptWidth Width { get; set; } = ReceiptWidth.Mm80;
}
