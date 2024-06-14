using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient(string.Empty, client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});
var app = builder.Build();

