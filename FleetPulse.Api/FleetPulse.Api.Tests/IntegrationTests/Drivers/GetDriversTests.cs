using Dapper;
using FleetPulse.Api.Tests.IntegrationTests.Infrastructure;
using FleetPulse.Contracts.Response.Drivers;
using FluentAssertions;
using Npgsql;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace FleetPulse.Api.Tests.IntegrationTests.Drivers
{
    [Collection("Integration")]
    public class GetDriversTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
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

        private static string Window(int fromMinutes) => 
            $"from={DateTime.UtcNow.AddMinutes(fromMinutes):o}&to={DateTime.UtcNow:o}";

        private async Task<List<LastestDriverStateResponse>> GetDriversAsync(string query)
        {
            var response = await Client.GetAsync($"/api/v1/drivers?{query}", CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var drivers = await response.Content.ReadFromJsonAsync<List<LastestDriverStateResponse>>(CancellationToken.None);
            drivers.Should().NotBeNull();
            return drivers!;
        }

        [Fact]
        public async Task GetDriversAsync_ShouldReturnExpectedCount()
        {
            var drivers = await GetDriversAsync(Window(-10));

            drivers.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetDriversAsync_ShouldReturnCorrectStatus()
        {
            var drivers = await GetDriversAsync(Window(-10));

            drivers.Single(d => d.DriverId == "driver1").Status.Should().Be("moving");
        }

        [Fact]
        public async Task GetDriversAsync_ShouldExcludeDriversOutsideFromWindow()
        {
            // driver-stale was last seen 30 minutes ago.
            var drivers = await GetDriversAsync(Window(-10));

            drivers.Should().NotContain(d => d.DriverId == "driver-stale");
        }

        [Fact]
        public async Task GetDriversAsync_ShouldReturnEmpty_WhenFromIsInTheFuture()
        {
            var query = $"from={DateTime.UtcNow.AddHours(1):o}&to={DateTime.UtcNow.AddHours(2):o}";

            var drivers = await GetDriversAsync(query);

            drivers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDriversAsync_ShouldMapFieldsCorrectly()
        {
            var drivers = await GetDriversAsync(Window(-10));

            var driver1 = drivers.Should().ContainSingle(d => d.DriverId == "driver1").Which;
            driver1.Status.Should().Be("moving");
            driver1.Latitude.Should().BeApproximately(37.7749, 0.0001);
            driver1.Longitude.Should().BeApproximately(-122.4194, 0.0001);
            driver1.Speed.Should().Be(50.0);
            driver1.LastSeen.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetDriversAsync_ShouldReturnBadRequest_WhenFromIsMissing()
        {
            var response = await Client.GetAsync("/api/v1/drivers", CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDriversAsync_ShouldReturnUnauthorized_WhenTokenIsInvalid()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/drivers?{Window(-10)}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid");

            var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
