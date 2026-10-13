using InventoryHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryHub.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(InventoryDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync(cancellationToken))
            return;

        var products = new[]
        {
            new Product { Name = "Wireless Mouse", Sku = "ACC-1001", Category = "Accessories", Price = 24.99m, StockQuantity = 84, Supplier = "Northstar Supply" },
            new Product { Name = "Mechanical Keyboard", Sku = "ACC-1002", Category = "Accessories", Price = 89.00m, StockQuantity = 31, Supplier = "Northstar Supply" },
            new Product { Name = "USB-C Hub", Sku = "ACC-1003", Category = "Accessories", Price = 42.50m, StockQuantity = 9, Supplier = "Aster Distribution" },
            new Product { Name = "27-inch Monitor", Sku = "DSP-2001", Category = "Displays", Price = 279.00m, StockQuantity = 14, Supplier = "Vision Wholesale" },
            new Product { Name = "Portable Monitor", Sku = "DSP-2002", Category = "Displays", Price = 189.99m, StockQuantity = 7, Supplier = "Vision Wholesale" },
            new Product { Name = "Laptop Stand", Sku = "ERG-3001", Category = "Ergonomics", Price = 39.99m, StockQuantity = 42, Supplier = "WorkWell Co." },
            new Product { Name = "Ergonomic Chair", Sku = "ERG-3002", Category = "Ergonomics", Price = 329.00m, StockQuantity = 5, Supplier = "WorkWell Co." },
            new Product { Name = "Noise-Cancelling Headphones", Sku = "AUD-4001", Category = "Audio", Price = 149.00m, StockQuantity = 26, Supplier = "Soundbridge Ltd." },
            new Product { Name = "USB Microphone", Sku = "AUD-4002", Category = "Audio", Price = 79.50m, StockQuantity = 0, Supplier = "Soundbridge Ltd." },
            new Product { Name = "1080p Webcam", Sku = "VID-5001", Category = "Video", Price = 64.00m, StockQuantity = 18, Supplier = "Aster Distribution" },
            new Product { Name = "4K Webcam", Sku = "VID-5002", Category = "Video", Price = 129.00m, StockQuantity = 11, Supplier = "Aster Distribution" },
            new Product { Name = "Laptop Sleeve", Sku = "BAG-6001", Category = "Bags", Price = 29.50m, StockQuantity = 57, Supplier = "Northstar Supply" },
            new Product { Name = "16-port Network Switch", Sku = "NET-7001", Category = "Networking", Price = 119.00m, StockQuantity = 8, Supplier = "Netwise Partners" },
            new Product { Name = "Wi-Fi 6 Access Point", Sku = "NET-7002", Category = "Networking", Price = 159.00m, StockQuantity = 4, Supplier = "Netwise Partners" },
            new Product { Name = "External SSD 1TB", Sku = "STO-8001", Category = "Storage", Price = 99.99m, StockQuantity = 22, Supplier = "DataCore Supply" }
        };

        foreach (var product in products)
            product.UpdatedAtUtc = DateTime.UtcNow;

        await db.Products.AddRangeAsync(products, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
