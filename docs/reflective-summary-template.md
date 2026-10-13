# Reflective summary — personalize before submitting

During the development of InventoryHub, I worked on connecting a Blazor WebAssembly front-end with an ASP.NET Core Minimal API back-end. I used `HttpClient` to send asynchronous requests and mapped the JSON responses to .NET models so that the interface could display product information, validation messages, loading states, and errors.

For the Copilot part of the assignment, I reviewed the integration flow, debugging steps, JSON contracts, and performance choices with Microsoft Copilot. **Before submitting, replace this sentence with the actual prompts you ran and the specific Copilot suggestions you accepted or rejected.** I checked suggestions against the project code rather than assuming that generated code was correct.

The integration uses a consistent paged response for product lists, a shared error-response shape, server-side filtering, and a short-lived cache that is invalidated after create, update, or delete operations. This helped me connect API design with front-end behavior and think about performance without ignoring data correctness.

This project improved my understanding of REST endpoints, asynchronous programming, JSON serialization, validation, CORS, caching, and debugging. One important lesson was that client-side validation improves usability, but the API must validate requests independently. A next step would be to add automated integration tests and authentication before deploying the application for real business use.
