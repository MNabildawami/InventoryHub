# InventoryHub — Full-Stack Integration Project

InventoryHub is a small inventory-management application built to demonstrate front-end/back-end integration through a REST API. The browser app is built with **Blazor WebAssembly**, the API uses **ASP.NET Core Minimal APIs**, and **Entity Framework Core with SQLite** provides persistent storage.

## Features

- Product catalog with search by name, SKU, or supplier.
- Category filtering and server-side pagination (up to 50 records per page).
- Create, update, and delete product records.
- Consistent JSON response shapes for product lists and API errors.
- Loading, success, empty, validation, and error states in the UI.
- Client-side form validation plus server-side validation.
- Duplicate-SKU checks and unique database index.
- Short-lived in-memory cache for product and summary reads; writes invalidate the cache.
- Swagger UI for exploring endpoints in Development.
- Seed data so the dashboard has products on first run.

## Technology stack

| Layer | Technology | Responsibility |
|---|---|---|
| Front-end | Blazor WebAssembly, Razor, CSS | UI, form validation, loading states, page navigation |
| Integration | `HttpClient`, JSON serialization | Sends HTTP requests and maps JSON to .NET models |
| Back-end | ASP.NET Core Minimal APIs | Filtering, pagination, validation, CRUD endpoints |
| Data | EF Core + SQLite | Persistent product records and a unique SKU index |
| Performance | `IMemoryCache` | Reduces repeated read work and invalidates after mutations |

## Requirements

- .NET 8 SDK
- A web browser
- Internet access for the optional Google Fonts import in the stylesheet; the app otherwise uses local assets and APIs

## Run locally

Open **two terminals** from the repository root.

**Terminal 1 — start the API:**

```bash
dotnet run --project src/InventoryHub.Api
```

The API listens at `http://localhost:5080`. In Development, open the Swagger page at `http://localhost:5080/swagger`.

**Terminal 2 — start the Blazor app:**

```bash
dotnet run --project src/InventoryHub.Web
```

Open `http://localhost:5200`. The web client reads its API base URL from `src/InventoryHub.Web/wwwroot/appsettings.json`.

The API creates `inventoryhub.db` in its content directory and seeds sample products on first launch. The database file is intentionally ignored by Git; the seeded data is recreated on a clean run.

> If you change either port, update the client `ApiBaseUrl` and the API's CORS origins in `src/InventoryHub.Api/Program.cs`.

## API endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| `GET` | `/api/health` | Health check |
| `GET` | `/api/summary` | Dashboard inventory totals |
| `GET` | `/api/categories` | Distinct categories |
| `GET` | `/api/products?search=mouse&category=Accessories&page=1&pageSize=8` | Search, filter, and paginate |
| `GET` | `/api/products/{id}` | Get one product |
| `POST` | `/api/products` | Create a product |
| `PUT` | `/api/products/{id}` | Update a product |
| `DELETE` | `/api/products/{id}` | Delete a product |

### JSON examples

A `GET /api/products?page=1&pageSize=1` response has this shape:

```json
{
  "items": [
    {
      "productId": 1,
      "name": "Wireless Mouse",
      "sku": "ACC-1001",
      "category": "Accessories",
      "price": 24.99,
      "stockQuantity": 84,
      "supplier": "Northstar Supply",
      "updatedAtUtc": "2026-10-13T00:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 2,
  "totalItems": 15,
  "totalPages": 8
}
```

`updatedAtUtc` is generated at runtime, so the exact timestamp will differ. Product lists always use the `items`, `page`, `pageSize`, `totalItems`, and `totalPages` envelope.

A validation or missing-record error uses this JSON structure:

```json
{
  "code": "product_not_found",
  "message": "Product 9999 was not found.",
  "traceId": null
}
```

## Integration and performance choices

1. **API service abstraction:** `ProductApiService` keeps HTTP and JSON handling out of the Razor page and interprets non-success responses consistently.
2. **Explicit JSON models:** the API and client have product and paged-response models to avoid rendering raw, unstructured JSON in the UI.
3. **Async requests:** API calls are awaited, and the page displays loading and error states during requests.
4. **Pagination and filtering:** the API returns smaller result sets rather than requiring the client to render the entire catalog.
5. **Short-lived cache:** product reads and summary reads reuse a 30-second in-memory cache entry. Create, update, and delete operations invalidate it to prevent stale results after a write.
6. **Validation:** the UI validates form values for usability, and the API validates them again because client-side validation is not a security boundary.

## Copilot activity notes

The assignment asks for evidence of Microsoft Copilot use in code generation, debugging, JSON handling, and performance optimization. The `docs/copilot-prompts.md` file includes focused prompts for reviewing these areas. Run those prompts in Copilot, compare its suggestions against the code, test any changes you accept, and record the actual suggestions you used. Do not claim that a suggestion was adopted unless you verified it.

See `docs/reflective-summary-template.md` for a reflection starter to personalize before submission.

## Publish to GitHub for peer review

1. Create a **public** repository named `InventoryHub` at <https://github.com/new>. Do not initialize a second README or `.gitignore`, because these files are already included.
2. Extract this project archive and open a terminal in the `InventoryHub` folder.
3. Run the following commands, replacing the example URL with your own repository URL:

```bash
git init
git add .
git commit -m "Initial commit: InventoryHub full-stack integration"
git branch -M main
git remote add origin https://github.com/YOUR-USERNAME/InventoryHub.git
git push -u origin main
```

4. Open the GitHub repository and confirm that the `src`, `docs`, workflow, and README files are visible. Submit that public repository URL to Coursera.

## Smoke test

After starting the API, PowerShell users can run the CRUD smoke test from the repository root:

```powershell
.\scripts\smoke-test.ps1
```

It creates a temporary product, updates it, deletes it, and reports whether each request succeeded. It does not require the Blazor UI to be running.

## Limitations and possible next steps

This is a learning/demo application, not a production inventory system. It does not implement login/role-based authorization, supplier management, stock-movement auditing, distributed caching, automated CI tests, or production hosting. The in-memory cache is local to a single API process. For production, add authentication/authorization, structured observability, automated integration tests, database migrations, and deployment-specific secrets/configuration.
