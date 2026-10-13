using System.Net;
using System.Net.Http.Json;
using InventoryHub.Web.Models;

namespace InventoryHub.Web.Services;

/// <summary>
/// Keeps HTTP and JSON details out of Razor components and gives the UI one place
/// to handle API response errors consistently.
/// </summary>
public sealed class ProductApiService(HttpClient http)
{
    public async Task<PagedResponse<Product>> GetProductsAsync(
        string? search,
        string? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"page={Math.Max(1, page)}",
            $"pageSize={Math.Clamp(pageSize, 1, 50)}"
        };

        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            query.Add($"category={Uri.EscapeDataString(category)}");

        using var response = await http.GetAsync($"api/products?{string.Join("&", query)}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<PagedResponse<Product>>(cancellationToken: cancellationToken)
            ?? new PagedResponse<Product>();
    }

    public async Task<InventorySummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync("api/summary", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<InventorySummary>(cancellationToken: cancellationToken)
            ?? new InventorySummary();
    }

    public async Task<List<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync("api/categories", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: cancellationToken) ?? new List<string>();
    }

    public async Task<Product> CreateAsync(ProductFormModel form, CancellationToken cancellationToken = default)
    {
        var request = ToRequest(form);
        using var response = await http.PostAsJsonAsync("api/products", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<Product>(cancellationToken: cancellationToken)
            ?? throw new ApiRequestException("The API returned an empty product response.", HttpStatusCode.BadGateway);
    }

    public async Task<Product> UpdateAsync(int id, ProductFormModel form, CancellationToken cancellationToken = default)
    {
        var request = ToRequest(form);
        using var response = await http.PutAsJsonAsync($"api/products/{id}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<Product>(cancellationToken: cancellationToken)
            ?? throw new ApiRequestException("The API returned an empty product response.", HttpStatusCode.BadGateway);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/products/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static ProductWriteRequest ToRequest(ProductFormModel form) => new(
        form.Name.Trim(), form.Sku.Trim(), form.Category.Trim(), form.Price,
        form.StockQuantity, form.Supplier.Trim());

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        ApiError? apiError = null;
        try
        {
            apiError = await response.Content.ReadFromJsonAsync<ApiError>(cancellationToken: cancellationToken);
        }
        catch (System.Text.Json.JsonException)
        {
            // Non-JSON error body: use the HTTP status as a safe fallback.
        }

        var message = apiError?.Message ?? (response.StatusCode switch
        {
            HttpStatusCode.BadRequest => "Please check the submitted product details.",
            HttpStatusCode.Conflict => "A product with these details already exists.",
            HttpStatusCode.NotFound => "The requested product could not be found.",
            _ => $"The API request failed (HTTP {(int)response.StatusCode})."
        });
        throw new ApiRequestException(message, response.StatusCode);
    }
}

public sealed class ApiRequestException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
