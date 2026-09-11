using Dapper;
using FleetPulse.Domain.Entities;
using FleetPulse.Domain.Enums;
using FleetPulse.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

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

        public async Task<IEnumerable<Alert>> GetAlertsByStatusDateRangeAsync(AlertStatus? status, RiskLevel? riskLevel, DateTime? startDate, DateTime? endDate, int pageSize, int pageNumber, CancellationToken cancellationToken)
        {
            var sql = """
                SELECT id, driver_id, event_latitude, event_longitude,
                       exit_speed, exit_time, zone_name, zone_type,
                       risk_level, assessment, recommendation, auto_escalate, status, raised_at
                FROM fleetpulse.alerts
                WHERE (@Status IS NULL OR status = CAST(@Status AS text))
                     AND (@RiskLevel IS NULL OR risk_level = CAST(@RiskLevel AS text))
                     AND (@StartDate IS NULL OR raised_at >= CAST(@StartDate AS timestamptz))
                     AND (@EndDate IS NULL OR raised_at <= CAST(@EndDate AS timestamptz))
                ORDER BY raised_at DESC
                LIMIT @Limit OFFSET @Offset
                """;

            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            var offset = (pageNumber - 1) * pageSize;
            var parameters = new DynamicParameters();
            parameters.Add("Status", status?.ToString(), DbType.String);
            parameters.Add("RiskLevel", riskLevel?.ToString(), DbType.String);
            parameters.Add("StartDate", startDate, DbType.DateTime);
            parameters.Add("EndDate", endDate, DbType.DateTime);
            parameters.Add("Limit", pageSize);
            parameters.Add("Offset", offset);

            var rows = await connection.QueryAsync<Alert>(sql, parameters);
            return rows;
        }
    }
}
