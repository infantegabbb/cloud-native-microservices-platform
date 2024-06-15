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

