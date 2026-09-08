using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FleetPulse.SignalRHub.Tests.IntegrationTests.Infrastructure
{
    public abstract class IntegrationTest : IAsyncLifetime
    {
        protected readonly IntegrationTestFixture Fixture;
        protected readonly HttpClient Client;

        protected IntegrationTest(IntegrationTestFixture fixture)
        {
            Fixture = fixture;
            Client = fixture.Client;
        }

        public async ValueTask InitializeAsync()
        {
            await CleanDatabase();
            await SeedDatabase();
        }


        public ValueTask DisposeAsync() => ValueTask.CompletedTask;


        private async Task CleanDatabase()
        {
            using var connection = new NpgsqlConnection(Fixture.ConnectionString);
            await connection.OpenAsync();
            await connection.ExecuteAsync("""
                TRUNCATE TABLE fleetpulse.gps_history, fleetpulse.driver_latest_state, fleetpulse.alerts;
                """);
        }

        private async Task SeedDatabase()
        {
            using var connection = new NpgsqlConnection(Fixture.ConnectionString);
            await connection.OpenAsync();
            await SeedDataAsync(connection);
        }

        protected abstract Task SeedDataAsync(NpgsqlConnection connection);
    }
}
