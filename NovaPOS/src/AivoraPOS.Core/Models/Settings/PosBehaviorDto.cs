using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Models.Settings;

public sealed class PosBehaviorDto
{
    public int IdleLockTimeoutMinutes { get; set; } = 5;
    public bool RequireManagerForDiscount { get; set; }
    public bool AllowNegativeStock { get; set; }
    public PaymentMethod DefaultPaymentMethod { get; set; } = PaymentMethod.Cash;
    public bool SoundOnSaleComplete { get; set; } = true;
}
