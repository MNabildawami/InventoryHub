using InventoryHub.Api.Data;
using InventoryHub.Api.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Inventory") ?? "Data Source=inventoryhub.db"));
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy("BlazorClient", policy =>
    policy.WithOrigins("http://localhost:5200", "https://localhost:7200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("InventoryHub.Api");
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    logger.LogError(exception, "Unhandled error while processing an InventoryHub API request.");
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new ApiError(
        "internal_error",
        "An unexpected server error occurred. Please try again later.",
        context.TraceIdentifier));
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("BlazorClient");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DatabaseSeeder.SeedAsync(db);
}

const string ProductCacheKey = "inventoryhub:products:all:v1";

app.MapGet("/api/summary", async (InventoryDbContext db, IMemoryCache cache, CancellationToken cancellationToken) =>
{
    var allProducts = await cache.GetOrCreateAsync(ProductCacheKey, async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
        entry.SlidingExpiration = TimeSpan.FromSeconds(15);
        return await db.Products.AsNoTracking().OrderBy(product => product.Name).ToListAsync(cancellationToken);
    }) ?? new List<Product>();

    var summary = new InventorySummary(
        allProducts.Count,
        allProducts.Count(product => product.StockQuantity is > 0 and <= 10),
        allProducts.Count(product => product.StockQuantity == 0),
        allProducts.Sum(product => product.Price * product.StockQuantity));
    return Results.Ok(summary);
})
.WithName("GetInventorySummary");

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", service = "InventoryHub.Api" }))
    .WithName("HealthCheck");


app.MapGet("/api/categories", async (InventoryDbContext db, CancellationToken cancellationToken) =>
{
    var categories = await db.Products.AsNoTracking()
        .Select(product => product.Category)
        .Distinct()
        .OrderBy(category => category)
        .ToListAsync(cancellationToken);
    return Results.Ok(categories);
})
.WithName("GetCategories");


app.MapGet("/api/products", async (
    string? search,
    string? category,
    int? page,
    int? pageSize,
    InventoryDbContext db,
    IMemoryCache cache,
    CancellationToken cancellationToken) =>
{
    var allProducts = await cache.GetOrCreateAsync(ProductCacheKey, async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
        entry.SlidingExpiration = TimeSpan.FromSeconds(15);
        return await db.Products.AsNoTracking()
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);
    }) ?? new List<Product>();

    IEnumerable<Product> filtered = allProducts;
    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim();
        filtered = filtered.Where(product =>
            product.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            product.Sku.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            product.Supplier.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "All", StringComparison.OrdinalIgnoreCase))
        filtered = filtered.Where(product => string.Equals(product.Category, category, StringComparison.OrdinalIgnoreCase));

    var productList = filtered.ToList();
    var currentPage = Math.Max(1, page ?? 1);
    var currentPageSize = Math.Clamp(pageSize ?? 8, 1, 50);
    var totalItems = productList.Count;
    var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)currentPageSize));
    currentPage = Math.Min(currentPage, totalPages);

    var items = productList
        .Skip((currentPage - 1) * currentPageSize)
        .Take(currentPageSize)
        .Select(ProductDto.FromEntity)
        .ToList();

    return Results.Ok(new PagedResponse<ProductDto>(items, currentPage, currentPageSize, totalItems, totalPages));
})
.WithName("GetProducts");


app.MapGet("/api/products/{id:int}", async (int id, InventoryDbContext db, CancellationToken cancellationToken) =>
{
    var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(item => item.ProductId == id, cancellationToken);
    return product is null
        ? Results.NotFound(new ApiError("product_not_found", $"Product {id} was not found."))
        : Results.Ok(ProductDto.FromEntity(product));
})
.WithName("GetProductById");

app.MapPost("/api/products", async (
    CreateProductRequest request,
    InventoryDbContext db,
    IMemoryCache cache,
    CancellationToken cancellationToken) =>
{
    var validationError = ValidateProduct(request.Name, request.Sku, request.Category, request.Price, request.StockQuantity, request.Supplier);
    if (validationError is not null)
        return Results.BadRequest(new ApiError("validation_error", validationError));

    var sku = request.Sku.Trim();
    if (await db.Products.AnyAsync(product => product.Sku == sku, cancellationToken))
        return Results.Conflict(new ApiError("duplicate_sku", $"SKU '{sku}' already exists."));

    var product = new Product
    {
        Name = request.Name.Trim(),
        Sku = sku,
        Category = request.Category.Trim(),
        Price = request.Price,
        StockQuantity = request.StockQuantity,
        Supplier = request.Supplier.Trim(),
        UpdatedAtUtc = DateTime.UtcNow
    };

    db.Products.Add(product);
    await db.SaveChangesAsync(cancellationToken);
    cache.Remove(ProductCacheKey);
    return Results.Created($"/api/products/{product.ProductId}", ProductDto.FromEntity(product));
})
.WithName("CreateProduct");

app.MapPut("/api/products/{id:int}", async (
    int id,
    UpdateProductRequest request,
    InventoryDbContext db,
    IMemoryCache cache,
    CancellationToken cancellationToken) =>
{
    var validationError = ValidateProduct(request.Name, request.Sku, request.Category, request.Price, request.StockQuantity, request.Supplier);
    if (validationError is not null)
        return Results.BadRequest(new ApiError("validation_error", validationError));

    var product = await db.Products.FirstOrDefaultAsync(item => item.ProductId == id, cancellationToken);
    if (product is null)
        return Results.NotFound(new ApiError("product_not_found", $"Product {id} was not found."));

    var sku = request.Sku.Trim();
    if (await db.Products.AnyAsync(item => item.ProductId != id && item.Sku == sku, cancellationToken))
        return Results.Conflict(new ApiError("duplicate_sku", $"SKU '{sku}' already exists."));

    product.Name = request.Name.Trim();
    product.Sku = sku;
    product.Category = request.Category.Trim();
    product.Price = request.Price;
    product.StockQuantity = request.StockQuantity;
    product.Supplier = request.Supplier.Trim();
    product.UpdatedAtUtc = DateTime.UtcNow;

    await db.SaveChangesAsync(cancellationToken);
    cache.Remove(ProductCacheKey);
    return Results.Ok(ProductDto.FromEntity(product));
})
.WithName("UpdateProduct");

app.MapDelete("/api/products/{id:int}", async (
    int id,
    InventoryDbContext db,
    IMemoryCache cache,
    CancellationToken cancellationToken) =>
{
    var product = await db.Products.FirstOrDefaultAsync(item => item.ProductId == id, cancellationToken);
    if (product is null)
        return Results.NotFound(new ApiError("product_not_found", $"Product {id} was not found."));

    db.Products.Remove(product);
    await db.SaveChangesAsync(cancellationToken);
    cache.Remove(ProductCacheKey);
    return Results.NoContent();
})
.WithName("DeleteProduct");

app.Run();

static string? ValidateProduct(string? name, string? sku, string? category, decimal price, int stockQuantity, string? supplier)
{
    if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120)
        return "Name is required and must be 120 characters or fewer.";
    if (string.IsNullOrWhiteSpace(sku) || sku.Trim().Length > 40)
        return "SKU is required and must be 40 characters or fewer.";
    if (string.IsNullOrWhiteSpace(category) || category.Trim().Length > 80)
        return "Category is required and must be 80 characters or fewer.";
    if (string.IsNullOrWhiteSpace(supplier) || supplier.Trim().Length > 120)
        return "Supplier is required and must be 120 characters or fewer.";
    if (price <= 0 || price > 9_999_999)
        return "Price must be greater than zero and no more than 9,999,999.";
    if (stockQuantity < 0 || stockQuantity > 10_000_000)
        return "Stock quantity must be between 0 and 10,000,000.";
    return null;
}
