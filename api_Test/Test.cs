using System.Net;
using System.Net.Http.Json;
using Xunit;

public class LoginRateLimitTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public LoginRateLimitTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_Should_Return_429_When_RateLimit_Exceeded()
    {
        var body = new
        {
            username = "testuser",
            password = "wrongpassword"
        };

        HttpResponseMessage lastResponse = null!;

        for (int i = 0; i < 6; i++)
        {
            lastResponse = await _client.PostAsJsonAsync(
                "/api/Account/login",
                body
            );
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_Should_Be_RateLimited()
    {
        var body = new
        {
            refreshToken = "fake-token"
        };

        HttpResponseMessage lastResponse = null!;

        for (int i = 0; i < 11; i++)
        {
            lastResponse = await _client.PostAsJsonAsync(
                "/api/Account/refresh-token",
                body
            );
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
    }

    [Fact]
    public async Task Unauthorized_User_Cannot_Create_Furniture()
    {
        var body = new { Name = "Chair", Price = 49.99, description = "A comfortable chair" };

        var response = await _client.PostAsJsonAsync(
            "/api/Furniture/AddFurniture",
            body
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
