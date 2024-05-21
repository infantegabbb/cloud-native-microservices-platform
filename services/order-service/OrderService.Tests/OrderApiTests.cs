using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace OrderService.Tests;

public class OrderApiTests
{
    [Fact]
    public async Task Health_ReturnsOk()
    {
        using var factory = CreateFactory(_ => OkUser());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", body.GetProperty("status").GetString());
        Assert.Equal("order-service", body.GetProperty("service").GetString());
    }

    [Fact]
    public async Task ListOrders_ReturnsSeedOrder()
    {
        using var factory = CreateFactory(_ => OkUser());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/orders");
        var orders = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, orders.GetArrayLength());
        Assert.Equal(1, orders[0].GetProperty("id").GetInt32());
        Assert.Equal("Mechanical Keyboard", orders[0].GetProperty("product").GetString());
    }

    [Theory]
    [InlineData(0, "Keyboard", 1)]
    [InlineData(1, "", 1)]
    [InlineData(1, "Keyboard", 0)]
    public async Task CreateOrder_InvalidInput_ReturnsBadRequest(int userId, string product, int quantity)
    {
        using var factory = CreateFactory(_ => OkUser());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/orders", new { userId, product, quantity });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_UnknownUser_ReturnsBadRequest()
    {
        using var factory = CreateFactory(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/orders", new { userId = 99, product = "Cloud Book", quantity = 1 });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("user does not exist", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task CreateOrder_UserServiceUnreachable_ReturnsServiceUnavailable()
    {
        using var factory = CreateFactory(_ => throw new HttpRequestException("user-service unreachable"));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/orders", new { userId = 1, product = "Cloud Book", quantity = 2 });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ExistingUser_ReturnsCreated()
    {
        using var factory = CreateFactory(_ => OkUser());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/orders", new { userId = 1, product = " Cloud Book ", quantity = 2 });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith("/orders/2", response.Headers.Location?.ToString() ?? string.Empty);
        Assert.Equal(2, body.GetProperty("id").GetInt32());
        Assert.Equal(1, body.GetProperty("userId").GetInt32());
        Assert.Equal("Cloud Book", body.GetProperty("product").GetString());
        Assert.Equal(2, body.GetProperty("quantity").GetInt32());
        Assert.Equal("created", body.GetProperty("status").GetString());
    }

    private static WebApplicationFactory<Program> CreateFactory(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var factoryStub = new StubHttpClientFactory(new StubHandler(handler));
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IHttpClientFactory>();
                services.AddSingleton<IHttpClientFactory>(factoryStub);
            });
        });
    }

    private static HttpResponseMessage OkUser() =>
        new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { id = 1, name = "Ada Lovelace", email = "ada@example.com" })
        };

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

