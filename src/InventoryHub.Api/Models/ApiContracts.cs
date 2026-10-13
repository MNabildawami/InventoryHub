namespace InventoryHub.Api.Models;

public sealed record ProductDto(
    int ProductId,
    string Name,
    string Sku,
    string Category,
    decimal Price,
    int StockQuantity,
    string Supplier,
    DateTime UpdatedAtUtc)
{
    public static ProductDto FromEntity(Product product) => new(
        product.ProductId,
        product.Name,
        product.Sku,
        product.Category,
        product.Price,
        product.StockQuantity,
        product.Supplier,
        product.UpdatedAtUtc);
}

public sealed record CreateProductRequest(
    string Name,
    string Sku,
    string Category,
    decimal Price,
    int StockQuantity,
    string Supplier);

public sealed record UpdateProductRequest(
    string Name,
    string Sku,
    string Category,
    decimal Price,
    int StockQuantity,
    string Supplier);

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record ApiError(string Code, string Message, string? TraceId = null);

public sealed record InventorySummary(int TotalProducts, int LowStockProducts, int OutOfStockProducts, decimal InventoryValue);
