using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaPOS.Core.Constants;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Core.Interfaces.Services;

namespace NovaPOS.Data.Services;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedAdminUserAsync(cancellationToken);
        await SeedDemoCashierAsync(cancellationToken);
        await SeedDefaultSettingsAsync(cancellationToken);
        await SeedSampleCatalogAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var admin = new User
        {
            FullName = SeedingDefaults.AdminFullName,
            Username = SeedingDefaults.AdminUsername,
            PinHash = _passwordHasher.HashPin(SeedingDefaults.DefaultAdminPin),
            PasswordHash = _passwordHasher.HashPassword(SeedingDefaults.DefaultAdminPassword),
            Role = UserRole.Admin,
            IsActive = true
        };

        await _context.Users.AddAsync(admin, cancellationToken);
        _logger.LogInformation("Seeded default admin account '{Username}'.", admin.Username);
    }

    private async Task SeedDemoCashierAsync(CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(x => x.Role == UserRole.Cashier, cancellationToken))
        {
            return;
        }

        var cashier = new User
        {
            FullName = "Demo Cashier",
            Username = "cashier",
            PinHash = _passwordHasher.HashPin("2468"),
            PasswordHash = _passwordHasher.HashPassword("Cashier@1234"),
            Role = UserRole.Cashier,
            IsActive = true
        };

        await _context.Users.AddAsync(cashier, cancellationToken);
        _logger.LogInformation("Seeded demo cashier account '{Username}'.", cashier.Username);
    }

    private async Task SeedDefaultSettingsAsync(CancellationToken cancellationToken)
    {
        if (await _context.AppSettings.AnyAsync(cancellationToken))
        {
            return;
        }

        var settings = new[]
        {
            new AppSetting { Key = "Store.Name", Value = "NovaPOS Demo Store" },
            new AppSetting { Key = "Store.Currency", Value = "USD" },
            new AppSetting { Key = "Tax.DefaultRate", Value = "0.0825" },
            new AppSetting { Key = "Receipt.Footer", Value = "Thank you for your business!" }
        };

        await _context.AppSettings.AddRangeAsync(settings, cancellationToken);
        _logger.LogInformation("Seeded default application settings.");
    }

    private async Task SeedSampleCatalogAsync(CancellationToken cancellationToken)
    {
        if (await _context.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var categories = new[]
        {
            new Category { Name = "Beverages", Description = "Hot and cold drinks" },
            new Category { Name = "Snacks", Description = "Packaged snacks and treats" },
            new Category { Name = "Grocery", Description = "Pantry staples" },
            new Category { Name = "Dairy", Description = "Milk, cheese, and eggs" },
            new Category { Name = "Household", Description = "Cleaning and household items" }
        };

        await _context.Categories.AddRangeAsync(categories, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var products = new List<Product>
        {
            CreateProduct(categories[0].Id, "BEV-001", "8412345678901", "Espresso", 1.20m, 3.50m, 50),
            CreateProduct(categories[0].Id, "BEV-002", "8412345678902", "Cappuccino", 1.50m, 4.25m, 40),
            CreateProduct(categories[0].Id, "BEV-003", "8412345678903", "Bottled Water", 0.60m, 1.99m, 120),
            CreateProduct(categories[1].Id, "SNK-001", "8412345678904", "Potato Chips", 0.90m, 2.49m, 80),
            CreateProduct(categories[1].Id, "SNK-002", "8412345678905", "Chocolate Bar", 0.70m, 1.79m, 60),
            CreateProduct(categories[2].Id, "GRC-001", "8412345678906", "Sandwich Bread", 1.80m, 3.99m, 30),
            CreateProduct(categories[2].Id, "GRC-002", "8412345678907", "Pasta (500g)", 1.10m, 2.49m, 45),
            CreateProduct(categories[3].Id, "DRY-001", "8412345678908", "Whole Milk (1L)", 1.40m, 2.89m, 45),
            CreateProduct(categories[3].Id, "DRY-002", "8412345678909", "Free-Range Eggs (12)", 2.60m, 4.99m, 25),
            CreateProduct(categories[4].Id, "HSH-001", "8412345678910", "Dish Soap", 1.25m, 3.29m, 35)
        };

        await _context.Products.AddRangeAsync(products, cancellationToken);
        _logger.LogInformation("Seeded {ProductCount} sample products across {CategoryCount} categories.",
            products.Count, categories.Length);
    }

    private static Product CreateProduct(
        Guid categoryId,
        string sku,
        string barcode,
        string name,
        decimal purchasePrice,
        decimal salePrice,
        int stockQuantity)
    {
        return new Product
        {
            CategoryId = categoryId,
            Sku = sku,
            Barcode = barcode,
            Name = name,
            PurchasePrice = purchasePrice,
            SalePrice = salePrice,
            TaxRate = 0.0825m,
            StockQuantity = stockQuantity,
            LowStockThreshold = 5,
            IsActive = true
        };
    }
}
