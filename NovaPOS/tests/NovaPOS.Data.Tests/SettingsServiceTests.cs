using NovaPOS.Core.Constants;
using NovaPOS.Core.Enums;

namespace NovaPOS.Data.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void SettingKeys_UseStableNames()
    {
        Assert.Equal("Store.Name", SettingKeys.StoreName);
        Assert.Equal("Tax.DefaultRate", SettingKeys.DefaultTaxRate);
        Assert.Equal("Session.IdleTimeoutMinutes", SettingKeys.IdleTimeoutMinutes);
        Assert.Equal("Receipt.Footer", SettingKeys.ReceiptFooter);
    }

    [Fact]
    public void ReceiptWidth_HasExpectedValues()
    {
        Assert.Equal(58, (int)ReceiptWidth.Mm58);
        Assert.Equal(80, (int)ReceiptWidth.Mm80);
        Assert.Equal(210, (int)ReceiptWidth.A4);
    }
}
