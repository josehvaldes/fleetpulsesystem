using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FleetPulse.Tests.Infrastructure
{

    public class FleetPulseWebApplicationFactory
    : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public FleetPulseWebApplicationFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:FleetPulseDb"] = _connectionString
                };

                config.AddInMemoryCollection(settings);
            });

        }
    }
}
