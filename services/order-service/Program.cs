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

