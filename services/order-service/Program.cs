using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient(string.Empty, client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});
var app = builder.Build();

var orders = new ConcurrentDictionary<int, Order>();
orders[1] = new Order(1, 1, "Mechanical Keyboard", 1, "created");
var nextId = 2;

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "order-service" }));
app.MapGet("/orders", () => Results.Ok(orders.Values.OrderBy(o => o.Id)));
app.MapGet("/orders/{id:int}", (int id) => orders.TryGetValue(id, out var order) ? Results.Ok(order) : Results.NotFound(new { error = "order not found" }));

app.MapPost("/orders", async (CreateOrder input, IHttpClientFactory clientFactory) =>
{
    if (input.UserId <= 0 || string.IsNullOrWhiteSpace(input.Product) || input.Quantity <= 0)
        return Results.BadRequest(new { error = "userId, product and positive quantity are required" });

    var userService = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://localhost:8080";
    try
    {
        var client = clientFactory.CreateClient();
        using var response = await client.GetAsync($"{userService}/users/{input.UserId}");
        if (!response.IsSuccessStatusCode)
            return Results.BadRequest(new { error = "user does not exist" });
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }

