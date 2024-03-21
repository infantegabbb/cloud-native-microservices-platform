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

