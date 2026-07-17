using Dapper;
using FleetPulse.SignalRHub.Model;
using Npgsql;

namespace FleetPulse.SignalRHub.Services
{
    public class DatabaseService(NpgsqlDataSource _dataSource, ILogger<DatabaseService> _logger) : IDatabaseService
    {
        public async Task<string> GetVersion(CancellationToken cancellationToken)
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            var version = await connection.ExecuteScalarAsync<string>("SELECT version();");
            return version ?? "Not Available";
        }

        public async Task<List<AlertDto>> GetAlertsAsync(DateTime startTime, DateTime endTime, int limit, CancellationToken cancellationToken)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
            var sql = "select * from ai_alerts where created_at between @StartTime and @EndTime limit @Limit";
            var alerts = await conn.QueryAsync<AlertDto>(sql, new { StartTime = startTime, EndTime = endTime, Limit = limit });
            return alerts.ToList();
        }

        public async Task<List<GpsPingDto>> GetGPSHistory(string driverId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
            var sql = "SELECT driver_id, latitude, longitude, speed, heading, timestamp " +
                "FROM gps_history WHERE driver_id = @DriverId AND timestamp BETWEEN @StartTime AND @EndTime";

            var pings = await conn.QueryAsync<GpsPingDto>(sql, new { DriverId = driverId, StartTime = startTime, EndTime = endTime });
            return pings.ToList();
        }

        public async Task<List<LatestDriverStateDto>> GetLatestDriverStatesAsync(DateTime after, CancellationToken cancellationToken)
        {
            await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
            var sql = "SELECT driver_id, latitude, longitude, speed, heading, last_seen, status " +
                "FROM driver_latest_state where last_seen > @After";
            // Execute the query and map the results to LatestDriverStateDto
            var lastStates = await conn.QueryAsync<LatestDriverStateDto>(sql, new { After = after });
            return lastStates.ToList();
        }
    }
}
