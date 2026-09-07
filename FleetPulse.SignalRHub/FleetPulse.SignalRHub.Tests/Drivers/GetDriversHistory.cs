using Dapper;
using FleetPulse.Contracts.Response;
using FleetPulse.SignalRHub.Tests.Infrastructure;
using FluentAssertions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace FleetPulse.SignalRHub.Tests.Drivers
{
    [Collection("Integration")]
    public class GetDriversHistory : IntegrationTest
    {
        public GetDriversHistory(IntegrationTestFixture fixture) : base(fixture)
        {
        }

        protected override async Task SeedDataAsync(NpgsqlConnection connection)
        {
            await connection.ExecuteAsync("""
                INSERT INTO fleetpulse.gps_history
                    (driver_id, timestamp, latitude, longitude, speed, heading, accuracy)
                VALUES
                    -- driver1: 3 pings within the test's 10-minute window
                    ('driver1', NOW() - INTERVAL '8 minutes',  37.7749, -122.4194, 42.0, 90,  5.0),
                    ('driver1', NOW() - INTERVAL '5 minutes',  37.7755, -122.4180, 44.5, 92,  4.5),
                    ('driver1', NOW() - INTERVAL '2 minutes',  37.7760, -122.4170, 40.0, 88,  6.0),

                    -- driver2: 1 ping in the window
                    ('driver2', NOW() - INTERVAL '4 minutes',  34.0522, -118.2437, 30.0, 180, 7.0),

                    -- driver1: 1 ping OUTSIDE the window (older than 'from')
                    ('driver1', NOW() - INTERVAL '30 minutes', 37.7700, -122.4200, 50.0, 95,  8.0);
                """);
        }

        [Fact]
        public async Task GetDriversHistoryAsync_ShouldReturnExpectedResults() 
        {
            var driverId = "driver1";
            var from = DateTime.UtcNow.AddMinutes(-10).ToString("o");
            var to = DateTime.UtcNow.ToString("o");

            var response = await Client.GetAsync($"/api/v1/drivers/{driverId}/history?from={from}&to={to}", CancellationToken.None);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var history = await response.Content.ReadFromJsonAsync<List<GpsPingResponse>>(CancellationToken.None);
            history.Should().NotBeNull();
            history.Should().HaveCount(3); // 3 in-window pings; the 30-minute-old ping is out of range
            history.Should().OnlyContain(p => p.DriverId == driverId);

        }
    }
}
