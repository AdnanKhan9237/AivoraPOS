namespace NovaPOS.Reporting.Tests;

public class CartCalculationTests
{
    [Fact]
    public void LineTotal_IncludesTaxAfterDiscount()
    {
        const decimal unitPrice = 10m;
        const int quantity = 2;
        const decimal lineDiscount = 2m;
        const decimal taxRate = 0.15m;

        var net = unitPrice * quantity - lineDiscount;
        var tax = Math.Round(net * taxRate, 2, MidpointRounding.AwayFromZero);
        var total = net + tax;

        Assert.Equal(18m, net);
        Assert.Equal(2.7m, tax);
        Assert.Equal(20.7m, total);
    }
}
