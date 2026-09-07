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
                    ('driver3', 40.7128, -74.0060, NULL, NULL, NOW(), 'offline'),
                    ('driver-stale', 51.5074, -0.1278, 12.0, 45, NOW() - INTERVAL '30 minutes', 'moving');
                """);
        }

        [Fact]
        public async Task GetDriversAsync_ShouldReturnExpectedCount() 
        {
            
            var from = DateTime.UtcNow.AddMinutes(-10).ToString("o");
            var to = DateTime.UtcNow.ToString("o");
            var response = await Client.GetAsync($"/api/v1/drivers?from={from}&to={to}", CancellationToken.None);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var drivers = await response.Content.ReadFromJsonAsync<List<LastestDriverStateResponse>>(CancellationToken.None);

            drivers.Should().NotBeNull();
            drivers.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetDriversAsync_ShouldReturnCorrectStatus() 
        {
            var from = DateTime.UtcNow.AddMinutes(-10).ToString("o");
            var to = DateTime.UtcNow.ToString("o");
            var response = await Client.GetAsync($"/api/v1/drivers?from={from}&to={to}", CancellationToken.None);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var drivers = await response.Content.ReadFromJsonAsync<List<LastestDriverStateResponse>>(CancellationToken.None);

            drivers.Should().NotBeNull();
            drivers.Should().HaveCountGreaterThan(0);
            var first = drivers.Single(d => d.DriverId == "driver1");
            first.Should().NotBeNull();
            first.Status.Should().Be("moving");
        }

        [Fact]
        public async Task GetDriversAsync_ShouldExcludeDriversOutsideFromWindow()
        {
            // driver-stale has last_seen 30 min ago; a 10-minute 'from' window must exclude it.
            var from = DateTime.UtcNow.AddMinutes(-10).ToString("o");
            var to = DateTime.UtcNow.ToString("o");

            var response = await Client.GetAsync($"/api/v1/drivers?from={from}&to={to}", CancellationToken.None);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var drivers = await response.Content.ReadFromJsonAsync<List<LastestDriverStateResponse>>(CancellationToken.None);

            drivers.Should().NotBeNull();
            drivers.Should().NotContain(d => d.DriverId == "driver-stale");
            drivers.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetDriversAsync_ShouldReturnEmpty_WhenFromIsInTheFuture()
        {
            var from = DateTime.UtcNow.AddHours(1).ToString("o");
            var to = DateTime.UtcNow.AddHours(2).ToString("o");

            var response = await Client.GetAsync($"/api/v1/drivers?from={from}&to={to}", CancellationToken.None);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var drivers = await response.Content.ReadFromJsonAsync<List<LastestDriverStateResponse>>(CancellationToken.None);

            drivers.Should().NotBeNull();
            drivers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDriversAsync_ShouldMapFieldsCorrectly()
        {
            var from = DateTime.UtcNow.AddMinutes(-10).ToString("o");
            var to = DateTime.UtcNow.ToString("o");

            var response = await Client.GetAsync($"/api/v1/drivers?from={from}&to={to}", CancellationToken.None);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var drivers = await response.Content.ReadFromJsonAsync<List<LastestDriverStateResponse>>(CancellationToken.None);

            var driver1 = drivers.Should().ContainSingle(d => d.DriverId == "driver1").Which;
            driver1.Status.Should().Be("moving");
            driver1.Latitude.Should().BeApproximately(37.7749, 0.0001);
            driver1.Longitude.Should().BeApproximately(-122.4194, 0.0001);
            driver1.Speed.Should().Be(50.0);
            driver1.LastSeen.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetDriverHistory_UnknownDriver_ShouldReturnEmpty()
        {
            var from = DateTime.UtcNow.AddMinutes(-10).ToString("o");
            var to = DateTime.UtcNow.ToString("o");

            var response = await Client.GetAsync($"/api/v1/drivers/no-such-driver/history?from={from}&to={to}", CancellationToken.None);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var history = await response.Content.ReadFromJsonAsync<List<GpsPingResponse>>(CancellationToken.None);

            history.Should().NotBeNull();
            history.Should().BeEmpty();
        }

    }
}
