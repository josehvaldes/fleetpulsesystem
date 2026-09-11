using Dapper;
using FleetPulse.Infrastructure.Services;
using FleetPulse.SignalRHub.Mapping;
using FleetPulse.SignalRHub.Tests.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;

namespace FleetPulse.SignalRHub.Tests.IntegrationTests.Alerts
{
    [Collection("Integration")]
    public class DatabaseServiceTests : IntegrationTest
    {
        public DatabaseServiceTests(IntegrationTestFixture fixture) : base(fixture)
        {
        }

        protected override async Task SeedDataAsync(NpgsqlConnection connection)
        {
            await connection.ExecuteAsync("""
                INSERT INTO fleetpulse.alerts
                    (driver_id, event_latitude, event_longitude, exit_speed, exit_time,
                     zone_name, zone_type, risk_level, assessment, recommendation,
                     status, auto_escalate, raised_at)
                VALUES
                    ('driver1', 37.7749, -122.4194, 45.0, NOW() - INTERVAL '12 minutes',
                     'Downtown Core', 'restricted', 'High',
                     'High-speed exit from restricted zone', 'Dispatch supervisor immediately',
                     'New', true, NOW() - INTERVAL '12 minutes'),

                    ('driver2', 34.0522, -118.2437, 30.0, NOW() - INTERVAL '10 minutes',
                     'Harbor District', 'restricted', 'Medium',
                     'Moderate speed near harbor gate', 'Notify driver to reduce speed',
                     'InProgress', false, NOW() - INTERVAL '10 minutes'),

                    ('driver3', 40.7128, -74.0060, 55.0, NOW() - INTERVAL '8 minutes',
                     'Financial Zone', 'high-risk', 'Low',
                     'Aggressive exit at high speed', 'Escalate to fleet manager',
                     'Resolved', true, NOW() - INTERVAL '8 minutes');
                """);
        }

        [Fact]
        public async Task GetAlertsByStatusDateRangeAsync_ShouldAllowNullStatusAndRiskLevel()
        {
            SqlMapping.RegisterSqlMappings();
            await using var dataSource = NpgsqlDataSource.Create(Fixture.ConnectionString);
            var databaseService = new DatabaseService(dataSource, NullLogger<DatabaseService>.Instance);

            var alerts = await databaseService.GetAlertsByStatusDateRangeAsync(
                null,
                null,
                null,
                null,
                pageSize: 2,
                pageNumber: 1,
                CancellationToken.None);

            alerts.Should().HaveCount(2);
            alerts.Should().BeInDescendingOrder(alert => alert.raised_at);
        }
    }
}
