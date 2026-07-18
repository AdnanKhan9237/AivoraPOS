using AivoraPOS.Core.Entities;

namespace AivoraPOS.Data.Tests;

public class EntityModelTests
{
    [Fact]
    public void Product_UsesDecimalForMoneyFields()
    {
        var product = new Product
        {
            PurchasePrice = 4.50m,
            SalePrice = 9.99m,
            TaxRate = 0.15m
        };

        Assert.Equal(9.99m, product.SalePrice);
        Assert.Equal(4.50m, product.PurchasePrice);
        Assert.Equal(0.15m, product.TaxRate);
    }

    [Fact]
    public void BaseEntity_UsesGuidPrimaryKey()
    {
        var user = new User();
        Assert.NotEqual(Guid.Empty, user.Id);
    }
}
