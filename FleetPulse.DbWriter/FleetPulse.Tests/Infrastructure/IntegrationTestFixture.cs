using Dapper;
using Testcontainers.PostgreSql;
using Xunit;

namespace FleetPulse.Tests.Infrastructure
{
    public sealed class IntegrationTestFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _database =
            new PostgreSqlBuilder("timescale/timescaledb:latest-pg18")
                .WithDatabase("fleetpulse_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

        //public HttpClient Client { get; private set; } = null!;
        public string ConnectionString => _database.GetConnectionString();

        public async ValueTask InitializeAsync()
        {
            await _database.StartAsync();

            await InitializeDatabase();

            //var factory = new FleetPulseWebApplicationFactory(
            //    _database.GetConnectionString());

            //Client = factory.CreateClient();
        }

        public async ValueTask DisposeAsync()
        {
            //Client.Dispose();

            await _database.DisposeAsync();
        }

        private async Task InitializeDatabase()
        {
            // Run the shared schema script (db/init.sql) that Docker and other tooling use.
            // It is linked into this project and copied to the test output directory.
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "db", "init.sql");
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException(
                    "Schema script not found. Ensure db/init.sql is copied to the test output directory.",
                    scriptPath);
            }

            var script = await File.ReadAllTextAsync(scriptPath);

            await using var connection = new Npgsql.NpgsqlConnection(_database.GetConnectionString());
            await connection.OpenAsync();
            await connection.ExecuteAsync(script);
        }

    }
}
