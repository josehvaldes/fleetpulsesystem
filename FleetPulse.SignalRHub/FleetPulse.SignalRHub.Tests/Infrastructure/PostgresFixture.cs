using Testcontainers.PostgreSql;
using Xunit;

namespace FleetPulse.SignalRHub.Tests.Infrastructure
{
    public sealed class PostgresFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container =
            new PostgreSqlBuilder("timescale/timescaledb:latest-pg18")
                .WithDatabase("fleetpulse_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

        public string ConnectionString =>
            _container.GetConnectionString();

        public async ValueTask InitializeAsync()
        {
            await _container.StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _container.DisposeAsync();
        }
    }
}
