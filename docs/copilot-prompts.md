# Microsoft Copilot review prompts

Use these prompts with the actual InventoryHub source in Microsoft Copilot. Keep a short note of what Copilot suggested, what you changed, and how you tested the result.

## 1. Generate and refine integration code

> Review `ProductApiService.cs` and `Home.razor`. Explain the complete request flow from the Blazor form or search controls to the ASP.NET Core Minimal API and back. Suggest one improvement that makes the integration easier to maintain. Show the smallest code change and explain why it is safe.

## 2. Debug an integration issue

> Review the API base URL in the Blazor app, CORS configuration in the API, and the `ProductApiService` request methods. List likely causes of a failed browser request and give a step-by-step debugging process. Do not assume a cause without evidence; identify what to inspect in the browser console and API logs.

## 3. Create and validate JSON structures

> Inspect the request/response models used by the InventoryHub API. Verify that create/update requests, the paged product response, and API error responses have predictable JSON structures. Point out any casing or nullability mismatch between API DTOs and Blazor models. Suggest tests or sample requests to validate the structures.

## 4. Optimize integration performance

> Review the product-list endpoint and the Blazor UI. Evaluate pagination, filtering, asynchronous requests, and the 30-second in-memory cache. Identify possible stale-data or cache-invalidation risks and propose measurable optimizations without removing validation or error handling.

## 5. Review before commit

> Review the proposed changes as a code reviewer. Separate confirmed bugs from optional improvements. Do not rewrite entire files unnecessarily. Provide a verification checklist that I can run locally before committing changes.
