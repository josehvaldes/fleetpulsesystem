using Dapper;
using FleetPulse.Contracts.Response;
using FleetPulse.SignalRHub.Tests.Infrastructure;
using FluentAssertions;
using Npgsql;
using System.Net.Http.Json;
using Xunit;

namespace FleetPulse.SignalRHub.Tests.Alerts
{
    [Collection("Integration")]
    public class GetAlertsTests : IntegrationTest
    {
        public GetAlertsTests(IntegrationTestFixture fixture) : base(fixture) { }
        

        protected override async Task SeedDataAsync(NpgsqlConnection connection)
        {
            await connection.ExecuteAsync("""
                INSERT INTO fleetpulse.alerts
                    (driver_id, event_latitude, event_longitude, exit_speed, exit_time,
                     zone_name, zone_type, risk_level, assessment, recommendation,
                     status, autoscale, raised_at)
                VALUES
                    ('driver1', 37.7749, -122.4194, 45.0, NOW() - INTERVAL '12 minutes',
                     'Downtown Core', 'restricted', 'High',
                     'High-speed exit from restricted zone', 'Dispatch supervisor immediately',
                     'New', true, NOW() - INTERVAL '12 minutes'),

                    ('driver2', 34.0522, -118.2437, 30.0, NOW() - INTERVAL '10 minutes',
                     'Harbor District', 'restricted', 'Medium',
                     'Moderate speed near harbor gate', 'Notify driver to reduce speed',
                     'New', false, NOW() - INTERVAL '10 minutes'),

                    ('driver3', 40.7128, -74.0060, 55.0, NOW() - INTERVAL '8 minutes',
                     'Financial Zone', 'high-risk', 'High',
                     'Aggressive exit at high speed', 'Escalate to fleet manager',
                     'New', true, NOW() - INTERVAL '8 minutes'),

                    ('driver1', 37.8044, -122.2712, 25.0, NOW() - INTERVAL '6 minutes',
                     'Port of Oakland', 'restricted', 'Low',
                     'Minor boundary crossing', 'Monitor; no action required',
                     'InProgress', false, NOW() - INTERVAL '6 minutes'),

                    ('driver2', 34.0195, -118.4912, 40.0, NOW() - INTERVAL '5 minutes',
                     'Santa Monica Pier', 'tourist', 'Medium',
                     'Pedestrian-heavy zone exit', 'Advise caution, reroute',
                     'InProgress', false, NOW() - INTERVAL '5 minutes'),

                    ('driver3', 40.7580, -73.9855, 20.0, NOW() - INTERVAL '3 minutes',
                     'Times Square', 'tourist', 'Low',
                     'Slow pass through tourist zone', 'Log for audit',
                     'Resolved', false, NOW() - INTERVAL '3 minutes'),

                    ('driver1', 37.6213, -122.3790, 60.0, NOW() - INTERVAL '2 minutes',
                     'SFO Airport', 'high-risk', 'High',
                     'Rapid exit near terminal', 'Immediate review required',
                     'Resolved', true, NOW() - INTERVAL '2 minutes'),

                    ('driver2', 34.2000, -118.3000, 35.0, NOW() - INTERVAL '1 minute',
                     'Glendale Suburbs', 'residential', 'Medium',
                     'Unexpected stop in residential area', 'Check driver status',
                     'OnError', false, NOW() - INTERVAL '1 minute');
                """);
        }

        [Fact]
        public async Task GetAlerts_ShouldReturnExpectedResults() 
        {
            var from = DateTime.UtcNow.AddMinutes(-15).ToString("o");
            var to = DateTime.UtcNow.ToString("o");
            var status = "OnError";
            var limit = 10;
            var response = await Client.GetAsync($"/api/v1/alerts?from={from}&to={to}&status={status}&limit={limit}", CancellationToken.None);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var alerts = await response.Content.ReadFromJsonAsync<List<AlertResponse>>(CancellationToken.None);
            alerts.Should().NotBeEmpty();
            alerts.Should().HaveCount(1);
            alerts.First().Status.Should().Be(status);
        }
    }
}
