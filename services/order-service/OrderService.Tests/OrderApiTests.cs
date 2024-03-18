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

