using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Entities;

public class Payment : BaseEntity
{
    public int SaleId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }

    public Sale Sale { get; set; } = null!;
}
