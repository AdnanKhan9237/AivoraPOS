using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Models.Settings;

public sealed class BusinessInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string CurrencySymbol { get; set; } = "$";
    public CurrencyPosition CurrencyPosition { get; set; } = CurrencyPosition.Before;
    public decimal DefaultTaxRate { get; set; } = 0.0825m;
}
