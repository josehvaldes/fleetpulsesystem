using Dapper;
using FleetPulse.Alerts.Infrastructure.Mapping;
using FleetPulse.Alerts.Infrastructure.Repositories;
using FleetPulse.Alerts.Tests.IntegrationTests.Infrastructure;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace FleetPulse.Alerts.Tests.IntegrationTests
{
    [Collection("Integration")]
    public class AlertsRepositoryTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        protected override async Task SeedDataAsync(NpgsqlConnection connection)
        {
            await connection.ExecuteAsync("""
                INSERT INTO alerts.alerts
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
            AlertsSqlMapping.RegisterSqlMappings();
            await using var dataSource = NpgsqlDataSource.Create(Fixture.ConnectionString);
            var repository = new AlertsRepository(dataSource);

            var alerts = await repository.GetAlertsByStatusDateRangeAsync(
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
