using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

public class ApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "THIS_IS_A_VERY_LONG_AND_SECURE_SECRET_KEY_WITH_AT_LEAST_64_CHARACTERS_123456",
                ["Jwt:RefreshKey"] = "ANOTHER_VERY_LONG_AND_SECURE_SECRET_KEY_FOR_REFRESH_TOKENS_1234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["ASPNETCORE_ENVIRONMENT"] = "Test"
            };

            config.AddInMemoryCollection(settings);
        });
    }
}
