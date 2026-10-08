using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FleetPulse.Api.Tests.IntegrationTests.Infrastructure
{
    public class FleetPulseWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            // appsettings.json leaves the local JWT settings empty and Program.cs validates them eagerly,
            // before ConfigureAppConfiguration sources are applied, so they must be host settings.
            builder.UseSetting("Authentication:Local:Jwt:Secret", "test-secret-key-long-enough-for-hmac-32+chars");
            builder.UseSetting("Authentication:Local:Jwt:Issuer", "test-issuer");
            builder.UseSetting("Authentication:Local:Jwt:Audience", "test-audience");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:FleetPulseDb"] = connectionString
                });
            });

            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
            });
        }
    }
}
