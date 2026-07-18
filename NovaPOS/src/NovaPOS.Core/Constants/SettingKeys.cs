namespace NovaPOS.Core.Constants;

public static class SettingKeys
{
    public const string StoreName = "Store.Name";
    public const string StoreAddress = "Store.Address";
    public const string StorePhone = "Store.Phone";
    public const string StoreLogoPath = "Store.LogoPath";
    public const string CurrencySymbol = "Store.CurrencySymbol";
    public const string CurrencyPosition = "Store.CurrencyPosition";
    public const string DefaultTaxRate = "Tax.DefaultRate";

    public const string ReceiptHeader = "Receipt.Header";
    public const string ReceiptFooter = "Receipt.Footer";
    public const string ReceiptShowLogo = "Receipt.ShowLogo";
    public const string ReceiptAutoPrint = "Receipt.AutoPrint";
    public const string ReceiptWidth = "Receipt.Width";
    public const string PrinterName = "Printer.Name";

    public const string IdleTimeoutMinutes = "Session.IdleTimeoutMinutes";
    public const string RequireManagerForDiscount = "POS.RequireManagerForDiscount";
    public const string AllowNegativeStock = "POS.AllowNegativeStock";
    public const string DefaultPaymentMethod = "POS.DefaultPaymentMethod";
    public const string SoundOnSaleComplete = "POS.SoundOnSaleComplete";

    public const string UiTheme = "UI.Theme";
}
