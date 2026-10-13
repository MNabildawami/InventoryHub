using InventoryHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryHub.Api.Data;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<Product>();
        product.HasKey(p => p.ProductId);
        product.Property(p => p.Name).HasMaxLength(120).IsRequired();
        product.Property(p => p.Sku).HasMaxLength(40).IsRequired();
        product.HasIndex(p => p.Sku).IsUnique();
        product.Property(p => p.Category).HasMaxLength(80).IsRequired();
        product.Property(p => p.Supplier).HasMaxLength(120).IsRequired();
        product.Property(p => p.Price).HasPrecision(18, 2);
    }
}
