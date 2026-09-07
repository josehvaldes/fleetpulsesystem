using Dapper;
using FleetPulse.Contracts.Response;
using FleetPulse.SignalRHub.Tests.Infrastructure;
using FluentAssertions;
using Npgsql;
using System.Net.Http.Json;
using Xunit;

namespace FleetPulse.SignalRHub.Tests.Drivers
{
    // Use GetDriversTests :IClassFixture = one fixture instance (one Postgres container + one app) per test class. 
    // Use ICollectionFixture = one fixture instance shared across all classes tagged [Collection("Integration")].
    [Collection("Integration")]
    public class GetDriversTests : IntegrationTest
    {
        public GetDriversTests(IntegrationTestFixture fixture) : base(fixture) {

        }

        protected override async Task SeedDataAsync(NpgsqlConnection connection)
        {
            await connection.ExecuteAsync("""
                INSERT INTO fleetpulse.driver_latest_state (driver_id, latitude, longitude, speed, heading, last_seen, status)
                VALUES 
                    ('driver1', 37.7749, -122.4194, 50.0, 90, NOW(), 'moving'),
                    ('driver2', 34.0522, -118.2437, 0.0, 0, NOW(), 'stopped'),
                    ('driver3', 40.7128, -74.0060, NULL, NULL, NOW(), 'offline');
                """);
        }

        [Fact]
        public async Task GetDriversAsync_ShouldReturnExpectedResults() 
        {
            
            var from = DateTime.UtcNow.AddMinutes(-10).ToString("o");
            var to = DateTime.UtcNow.ToString("o");
            var response = await Client.GetAsync($"/api/v1/drivers?from={from}&to={to}", CancellationToken.None);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var drivers = await response.Content.ReadFromJsonAsync<List<LastestDriverStateResponse>>(CancellationToken.None);

            drivers.Should().NotBeNull();
            drivers.Should().HaveCount(3);
            drivers.First().DriverId.Should().Be("driver1");
            //drivers.ElementAt(0).Status.Should().Be("moving");
        }


    }
}
