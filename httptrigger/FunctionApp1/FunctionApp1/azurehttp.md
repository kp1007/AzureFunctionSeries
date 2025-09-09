# ⚡ Azure Functions at the Core: Serverless Design Series, Part 1

## Function Signatures & Return Types

**Function signature:** Uses **HttpRequestData** instead of HttpRequest

### Return Type Options:
- **HttpResponseData** ✅ (When you need HTTP control)
- **IActionResult** ⚠️ (Legacy, for in-process model, avoid)
- ✅ **Direct POCO returns** (Any simple C# class or record with properties: Define DTOs/Response Models)

(`T`, `Task<T>`, `T?`) are the most modern and use `HttpResponseData` when you need specific HTTP control

## Examples

### 1. Basic POCO Return with Request Body Binding

```csharp
// Define your POCO (Plain Old CLR Object)
public record CreateUserRequest(string Name, string Email, int Age);

[Function("CreateUser")]
public async Task<User> CreateUser(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
    [FromBody] CreateUserRequest userData)  // 👈 Automatic binding!
{
    // userData is already parsed, deserialized, and ready to use!
    return new User(userData.Name, userData.Email);
}
```

### 2. Multiple Parameter Binding (Route, Body, Query)

```csharp
// URL: /api/products/123?includeReviews=true
// Body: { "name": "Updated Product", "price": 99.99 }

public record UpdateRequest(string Name, decimal Price);
public record QueryOptions(bool IncludeReviews);

[Function("UpdateProduct")]
public Product UpdateProduct(
    [HttpTrigger(..., Route = "products/{id:int}")] HttpRequestData req,
    int id,                                  // From route
    [FromBody] UpdateRequest updateData,    // From JSON body
    [FromQuery] QueryOptions options)       // From query string
{
    // Everything is automatically bound and typed!
}
```

### 3. Complex Response with Service Integration

```csharp
public record ProductListResponse(IEnumerable<ProductResponse> Products, int TotalCount);

public class ProductFunctions
{
    private readonly IProductService _productService;
    
    [Function("GetAllProducts")]
    public async Task<ProductListResponse> GetAllProducts(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")] HttpRequestData req)
    {
        var products = await _productService.GetAllAsync();
        var responses = products.Select(p => new ProductResponse(p.Id, p.Name, p.Price, p.Stock));
        return new ProductListResponse(responses, responses.Count());
    }
}
```

### 4. Nullable Return Types

```csharp
[Function("GetOrder")]
public async Task<Order?> GetOrder(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{id:guid}")] HttpRequestData req,
    Guid id)
{
    _logger.LogInformation("Getting order {OrderId}", id);
    //return order or null
    return await _orderService.GetOrderAsync(id);
}
```

**Similarly, as per your use case you can use like `IEnumerable<T>`**

## Best Practices

### ❌ BAD: Using `object` Return Type

```csharp
[Function("GetUser")]
public object GetUser(...)  // ❌ No type safety
{
    return new { 
        id = 1, 
        name = "John" 
    };  // Anonymous type - poor for maintenance
}
```

### ✅ GOOD: HttpResponseData (When you need HTTP control)

```csharp
// HttpResponseData - When you need HTTP control
[Function("Delete")]
public HttpResponseData Delete(
    [HttpTrigger(...)] HttpRequestData req)
{
    // Return 204 No Content for successful deletion
    return req.CreateResponse(HttpStatusCode.NoContent);
}

// Task<HttpResponseData> - Async with HTTP control
[Function("Upload")]
public async Task<HttpResponseData> Upload(
    [HttpTrigger(...)] HttpRequestData req)
{
    var response = req.CreateResponse(HttpStatusCode.Created);
    response.Headers.Add("Location", "/api/items/123");
    await response.WriteAsJsonAsync(new { id = 123 });
    return response;
}
```
