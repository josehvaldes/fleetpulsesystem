using Dapper;
using Npgsql;
using Xunit;

namespace FleetPulse.Alerts.Tests.IntegrationTests.Infrastructure
{
    public abstract class IntegrationTest(IntegrationTestFixture fixture) : IAsyncLifetime
    {
        protected readonly IntegrationTestFixture Fixture = fixture;
        protected readonly HttpClient Client = fixture.Client;

        public async ValueTask InitializeAsync()
        {
            await using var connection = new NpgsqlConnection(Fixture.ConnectionString);
            await connection.OpenAsync();
            await connection.ExecuteAsync("""
                TRUNCATE TABLE fleetpulse.gps_history, alerts.alerts;
                """);

            await SeedDataAsync(connection);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        protected abstract Task SeedDataAsync(NpgsqlConnection connection);
    }
}
