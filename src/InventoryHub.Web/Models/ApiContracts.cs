namespace InventoryHub.Web.Models;

public sealed class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; } = 1;
}

public sealed class ApiError
{
    public string Code { get; set; } = "api_error";
    public string Message { get; set; } = "The request could not be completed.";
    public string? TraceId { get; set; }
}

public sealed class ProductFormModel
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Sku { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Category { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; } = 1;

    [Range(0, 10000000, ErrorMessage = "Stock cannot be negative.")]
    public int StockQuantity { get; set; }

    [Required, StringLength(120)]
    public string Supplier { get; set; } = string.Empty;

    public static ProductFormModel FromProduct(Product product) => new()
    {
        Name = product.Name,
        Sku = product.Sku,
        Category = product.Category,
        Price = product.Price,
        StockQuantity = product.StockQuantity,
        Supplier = product.Supplier
    };
}

public sealed record ProductWriteRequest(
    string Name,
    string Sku,
    string Category,
    decimal Price,
    int StockQuantity,
    string Supplier);

public sealed class InventorySummary
{
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal InventoryValue { get; set; }
}
