using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Tests;

public class EntityModelTests
{
    [Fact]
    public void Product_UsesDecimalForMoneyFields()
    {
        var product = new Product
        {
            UnitPrice = 9.99m,
            CostPrice = 4.50m,
            TaxRate = 0.0825m
        };

        Assert.Equal(9.99m, product.UnitPrice);
        Assert.Equal(4.50m, product.CostPrice);
        Assert.Equal(0.0825m, product.TaxRate);
    }
}
