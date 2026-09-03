using Dapper;
using FleetPulse.Domain.Entities;
using FleetPulse.Domain.Enums;
using FleetPulse.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FleetPulse.Infrastructure.Services
{
    public class DatabaseService(NpgsqlDataSource _dataSource, ILogger<DatabaseService> _logger) : IDatabaseService
    {
        public async Task<string> GetVersion(CancellationToken cancellationToken)
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            var version = await connection.ExecuteScalarAsync<string>("SELECT version();");
            _logger.LogInformation("Database version: {Version}", version);
            return version ?? "Not Available";
        }

        public async Task<IEnumerable<Alert>> GetAlertsAsync(DateTime startTime, DateTime endTime, int limit, CancellationToken cancellationToken)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
            var sql = "select * from fleetpulse.alerts where created_at between @StartTime and @EndTime limit @Limit";
            var alerts = await conn.QueryAsync<Alert>(sql, new { StartTime = startTime, EndTime = endTime, Limit = limit });
            return alerts;
        }

        public async Task<IEnumerable<GpsPing>> GetGPSHistory(string driverId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
            var sql = "SELECT driver_id, latitude, longitude, speed, heading, timestamp " +
                "FROM fleetpulse.gps_history WHERE driver_id = @DriverId AND timestamp BETWEEN @StartTime AND @EndTime";

            var pings = await conn.QueryAsync<GpsPing>(sql, new { DriverId = driverId, StartTime = startTime, EndTime = endTime });
            return pings;
        }

        public async Task<IEnumerable<LatestDriverState>> GetLatestDriverStatesAsync(DateTime after, CancellationToken cancellationToken)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
            var sql = "SELECT driver_id, latitude, longitude, speed, heading, last_seen, status " +
                "FROM fleetpulse.driver_latest_state where last_seen > @After";
            // Execute the query and map the results to LatestDriverState
            var lastStates = await conn.QueryAsync<LatestDriverState>(sql, new { After = after });
            return lastStates;
        }

        public async Task<IEnumerable<Alert>> GetAlertsByStatusDateRangeAsync(AlertStatus status, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var sql = """
                SELECT id, driver_id, event_latitude, event_longitude,
                       exit_speed, exit_time, zone_name, zone_type,
                       risk_level, assessment, recommendation, autoscale, status, raised_at
                FROM fleetpulse.alerts
                WHERE status = @Status AND raised_at >= @StartDate AND raised_at <= @EndDate
                ORDER BY raised_at DESC
                """;

            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            var rows = await connection.QueryAsync<Alert>(sql, new { Status = status, StartDate = startDate, EndDate = endDate });
            return rows;
        }
    }
}
