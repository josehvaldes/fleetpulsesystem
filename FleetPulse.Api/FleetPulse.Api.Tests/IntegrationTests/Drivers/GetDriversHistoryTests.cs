using Dapper;
using FleetPulse.Api.Tests.IntegrationTests.Infrastructure;
using FleetPulse.Contracts.Response.Drivers;
using FluentAssertions;
using Npgsql;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FleetPulse.Api.Tests.IntegrationTests.Drivers
{
    [Collection("Integration")]
    public class GetDriversHistoryTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        protected override async Task SeedDataAsync(NpgsqlConnection connection)
        {
            await connection.ExecuteAsync("""
                INSERT INTO fleetpulse.gps_history
                    (driver_id, timestamp, latitude, longitude, speed, heading, accuracy)
                VALUES
                    -- driver1: 3 pings within the 10-minute window
                    ('driver1', NOW() - INTERVAL '8 minutes',  37.7749, -122.4194, 42.0, 90,  5.0),
                    ('driver1', NOW() - INTERVAL '5 minutes',  37.7755, -122.4180, 44.5, 92,  4.5),
                    ('driver1', NOW() - INTERVAL '2 minutes',  37.7760, -122.4170, 40.0, 88,  6.0),

                    -- driver2: 1 ping in the window
                    ('driver2', NOW() - INTERVAL '4 minutes',  34.0522, -118.2437, 30.0, 180, 7.0),

                    -- driver1: 1 ping outside the window
                    ('driver1', NOW() - INTERVAL '30 minutes', 37.7700, -122.4200, 50.0, 95,  8.0);
                """);
        }

        private static string Window(int fromMinutes) =>
            $"from={DateTime.UtcNow.AddMinutes(fromMinutes):o}&to={DateTime.UtcNow:o}";

        private async Task<List<GpsPingResponse>> GetHistoryAsync(string driverId, string query)
        {
            var response = await Client.GetAsync($"/api/v1/drivers/{driverId}/history?{query}", CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var history = await response.Content.ReadFromJsonAsync<List<GpsPingResponse>>(CancellationToken.None);
            history.Should().NotBeNull();
            return history!;
        }

        [Fact]
        public async Task GetDriversHistoryAsync_ShouldReturnExpectedResults()
        {
            var history = await GetHistoryAsync("driver1", Window(-10));

            history.Should().HaveCount(3);
            history.Should().OnlyContain(p => p.DriverId == "driver1");
        }

        [Fact]
        public async Task GetDriversHistoryAsync_ShouldIncludeOlderPings_WhenWindowIsWideEnough()
        {
            var history = await GetHistoryAsync("driver1", Window(-60));

            history.Should().HaveCount(4);
        }

        [Fact]
        public async Task GetDriversHistoryAsync_ShouldReturnOnlyRequestedDriver()
        {
            var history = await GetHistoryAsync("driver2", Window(-10));

            history.Should().ContainSingle(p => p.DriverId == "driver2");
        }

        [Fact]
        public async Task GetDriversHistoryAsync_ShouldReturnEmpty_WhenFromIsInTheFuture()
        {
            var query = $"from={DateTime.UtcNow.AddHours(1):o}&to={DateTime.UtcNow.AddHours(2):o}";

            var history = await GetHistoryAsync("driver1", query);

            history.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDriversHistoryAsync_ShouldReturnEmpty_WhenDriverIsUnknown()
        {
            var history = await GetHistoryAsync("no-such-driver", Window(-10));

            history.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDriversHistoryAsync_ShouldMapFieldsCorrectly()
        {
            var history = await GetHistoryAsync("driver1", Window(-10));

            var ping = history.Should().ContainSingle(p => p.Latitude == 37.7749 && p.Longitude == -122.4194).Which;
            ping.DriverId.Should().Be("driver1");
            ping.Speed.Should().Be(42.0);
            ping.Heading.Should().Be(90);
            DateTime.TryParse(ping.Timestamp, out _).Should().BeTrue();
        }

        [Fact]
        public async Task GetDriversHistoryAsync_ShouldReturnBadRequest_WhenFromIsMissing()
        {
            var response = await Client.GetAsync($"/api/v1/drivers/driver1/history?to={DateTime.UtcNow:o}", CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDriversHistoryAsync_ShouldReturnBadRequest_WhenDatesAreInvalid()
        {
            var response = await Client.GetAsync("/api/v1/drivers/driver1/history?from=not-a-date&to=also-not-a-date", CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
