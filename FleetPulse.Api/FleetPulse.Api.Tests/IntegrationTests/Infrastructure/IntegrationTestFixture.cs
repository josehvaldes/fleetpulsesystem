using Dapper;
using Testcontainers.PostgreSql;
using Xunit;

namespace FleetPulse.Api.Tests.IntegrationTests.Infrastructure
{
    public sealed class IntegrationTestFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _database =
            new PostgreSqlBuilder("timescale/timescaledb:latest-pg18")
                .WithDatabase("fleetpulse_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

        private FleetPulseWebApplicationFactory _factory = null!;

        public HttpClient Client { get; private set; } = null!;
        public string ConnectionString => _database.GetConnectionString();

        public async ValueTask InitializeAsync()
        {
            await _database.StartAsync();

            await InitializeDatabase();

            _factory = new FleetPulseWebApplicationFactory(ConnectionString);
            Client = _factory.CreateClient();
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _factory.DisposeAsync();
            await _database.DisposeAsync();
        }

        private async Task InitializeDatabase()
        {
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "db", "init.sql");
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException(
                    "Schema script not found. Ensure db/init.sql is copied to the test output directory.",
                    scriptPath);
            }

            var script = await File.ReadAllTextAsync(scriptPath);

            await using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            await connection.ExecuteAsync(script);
        }
    }
}
