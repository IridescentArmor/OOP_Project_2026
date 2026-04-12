using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Marketplace.API.Data;
using Marketplace.API.DTOs;
using Marketplace.Tests.Support;

namespace Marketplace.Tests.Api;

[Collection("ApiIntegration")]
public class ApiIntegrationTests
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostProduct_PriceZero_Returns400_AndValidationTitle()
    {
        var body = new CreateProductRequest
        {
            Title = "Товар",
            Description = "Опис",
            Price = 0,
            StockQuantity = 1,
            CategoryId = Guid.NewGuid(),
            SellerId = Guid.NewGuid()
        };

        var res = await _client.PostAsJsonAsync("/api/products", body);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var json = await res.Content.ReadAsStringAsync();
        Assert.Contains("Помилка валідації", json);
    }

    [Fact]
    public async Task PostUser_DuplicateEmail_Returns400_ProblemDetails()
    {
        var email = $"dup-{Guid.NewGuid():N}@test.local";
        var body = new CreateUserRequest
        {
            Name = "Користувач",
            Email = email,
            PhoneNumber = "+380501112233",
            PasswordHash = "password123",
            IncludeCustomerRole = true
        };

        var first = await _client.PostAsJsonAsync("/api/users", body);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/users", body);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        var json = await second.Content.ReadAsStringAsync();
        Assert.Contains("Операція неможлива", json);
        Assert.Contains("існує", json);
    }

    [Fact]
    public async Task PostOrder_AfterLogin_Returns200()
    {
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = DbInitializer.DemoBuyerEmail,
            Password = DbInitializer.DemoBuyerPassword
        });
        loginRes.EnsureSuccessStatusCode();

        var auth = await loginRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth?.AccessToken);
        Assert.NotNull(auth.User); 

        var productsRes = await _client.GetFromJsonAsync<List<ProductResponse>>("/api/products");
        Assert.NotNull(productsRes);
        Assert.NotEmpty(productsRes);

        var orderBody = new CreateOrderRequest
        {
            UserId = auth.User.Id,
            Items = new Dictionary<Guid, int> { { productsRes[0].Id, 1 } }
        };

        var orderReq = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = JsonContent.Create(orderBody)
        };
        orderReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var res = await _client.SendAsync(orderReq);


        if (!res.IsSuccessStatusCode)
        {
            var errorDetail = await res.Content.ReadAsStringAsync();
            throw new Exception($"Request failed: {errorDetail}");
        }

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task PostOrder_WithoutToken_Returns401()
    {
        var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = null;

        var productsRes = await client.GetFromJsonAsync<List<ProductResponse>>("/api/products");
        Assert.NotNull(productsRes);
        Assert.NotEmpty(productsRes);

        var res = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest
        {
            Items = new Dictionary<Guid, int> { { productsRes[0].Id, 1 } }
        });

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
