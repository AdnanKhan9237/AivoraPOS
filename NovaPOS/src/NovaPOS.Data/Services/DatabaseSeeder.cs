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
            Username = SeedingDefaults.AdminUsername,
            DisplayName = SeedingDefaults.AdminDisplayName,
            PinHash = _passwordHasher.HashPin(SeedingDefaults.DefaultAdminPin),
            PasswordHash = _passwordHasher.HashPassword(SeedingDefaults.DefaultAdminPassword),
            Role = UserRole.Admin,
            IsActive = true
        };

        await _context.Users.AddAsync(admin, cancellationToken);
        _logger.LogInformation("Seeded default admin account '{Username}'.", admin.Username);
    }

    private async Task SeedDefaultSettingsAsync(CancellationToken cancellationToken)
    {
        if (await _context.AppSettings.AnyAsync(cancellationToken))
        {
            return;
        }

        var settings = new[]
        {
            new AppSetting
            {
                Key = "Store.Name",
                Value = "NovaPOS Demo Store",
                Category = "Store",
                Description = "Store display name on receipts"
            },
            new AppSetting
            {
                Key = "Store.Currency",
                Value = "USD",
                Category = "Store",
                Description = "ISO currency code"
            },
            new AppSetting
            {
                Key = "Tax.DefaultRate",
                Value = "0.0825",
                Category = "Tax",
                Description = "Default sales tax rate (decimal)"
            },
            new AppSetting
            {
                Key = "Receipt.Footer",
                Value = "Thank you for your business!",
                Category = "Receipt",
                Description = "Receipt footer message"
            }
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

        var beverages = new Category { Name = "Beverages", Description = "Hot and cold drinks", SortOrder = 1 };
        var snacks = new Category { Name = "Snacks", Description = "Packaged snacks", SortOrder = 2 };
        var grocery = new Category { Name = "Grocery", Description = "Pantry staples", SortOrder = 3 };

        await _context.Categories.AddRangeAsync([beverages, snacks, grocery], cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var products = new List<Product>
        {
            CreateProduct(beverages.Id, "BEV-001", "8412345678901", "Espresso", 3.50m, 1.20m, 50, 10),
            CreateProduct(beverages.Id, "BEV-002", "8412345678902", "Cappuccino", 4.25m, 1.50m, 40, 10),
            CreateProduct(beverages.Id, "BEV-003", "8412345678903", "Bottled Water", 1.99m, 0.60m, 120, 24),
            CreateProduct(snacks.Id, "SNK-001", "8412345678904", "Potato Chips", 2.49m, 0.90m, 80, 20),
            CreateProduct(snacks.Id, "SNK-002", "8412345678905", "Chocolate Bar", 1.79m, 0.70m, 60, 15),
            CreateProduct(grocery.Id, "GRC-001", "8412345678906", "Sandwich Bread", 3.99m, 1.80m, 30, 8),
            CreateProduct(grocery.Id, "GRC-002", "8412345678907", "Whole Milk (1L)", 2.89m, 1.40m, 45, 12),
            CreateProduct(grocery.Id, "GRC-003", "8412345678908", "Free-Range Eggs (12)", 4.99m, 2.60m, 25, 6)
        };

        await _context.Products.AddRangeAsync(products, cancellationToken);
        _logger.LogInformation("Seeded {Count} sample products across {CategoryCount} categories.",
            products.Count, 3);
    }

    private static Product CreateProduct(
        int categoryId,
        string sku,
        string barcode,
        string name,
        decimal price,
        decimal cost,
        decimal quantity,
        decimal reorderLevel)
    {
        return new Product
        {
            CategoryId = categoryId,
            Sku = sku,
            Barcode = barcode,
            Name = name,
            UnitPrice = price,
            CostPrice = cost,
            TaxRate = 0.0825m,
            IsActive = true,
            TrackInventory = true,
            Inventory = new Inventory
            {
                QuantityOnHand = quantity,
                ReorderLevel = reorderLevel,
                LastRestockedAt = DateTime.UtcNow
            }
        };
    }
}
