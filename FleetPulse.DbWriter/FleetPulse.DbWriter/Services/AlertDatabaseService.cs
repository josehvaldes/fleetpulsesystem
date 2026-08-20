using Dapper;
using FleetPulse.DbWriter.MetricsConfig;
using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Models.DB;
using FleetPulse.DbWriter.Services.Interfaces;
using Npgsql;

namespace FleetPulse.DbWriter.Services
{
    public class AlertDatabaseService(NpgsqlDataSource _dataSource,
        ILogger<AlertDatabaseService> _logger) : IAlertDatabaseService
    {
        public async Task<Guid> AddAlert(AlertDb alert)
        {
            if (alert.id == Guid.Empty)
                alert.id = Guid.NewGuid();

            var sql = """
                INSERT INTO fleetpulse.alerts (
                    id, driver_id,
                    event_latitude, event_longitude,
                    exit_speed, exit_time,
                    zone_name, zone_type,
                    risk_level, assessment, recommendation,
                    status,
                    autoscale, raised_at
                ) VALUES (
                    @id, @driver_id,
                    @event_latitude, @event_longitude,
                    @exit_speed, @exit_time,
                    @zone_name, @zone_type,
                    @risk_level, @assessment, @recommendation,
                    @status,
                    @autoscale, @raised_at
                )
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await connection.ExecuteAsync(sql, alert);
            FleetMetrics.AlertsProcessed.Inc();
            _logger.LogInformation("Alert added to database with ID: {AlertId}", alert.id);
            return alert.id;

        }

        public async Task<IEnumerable<AlertDb>> GetAlertsByDriverId(string driverId)
        {
            var sql = """
                SELECT id, driver_id, event_latitude, event_longitude,
                       exit_speed, exit_time, zone_name, zone_type,
                       risk_level, assessment, recommendation, autoscale, status, raised_at
                FROM fleetpulse.alerts
                WHERE driver_id = @DriverId
                ORDER BY raised_at DESC
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            var rows = await connection.QueryAsync<AlertDb>(sql, new { DriverId = driverId });
            return rows;
        }

        public async Task<IEnumerable<AlertDb>> GetAlertsByDateRange(DateTime startDate, DateTime endDate)
        {
            var sql = """
                SELECT id, driver_id, event_latitude, event_longitude,
                       exit_speed, exit_time, zone_name, zone_type,
                       risk_level, assessment, recommendation, autoscale, status, raised_at
                FROM fleetpulse.alerts
                WHERE raised_at >= @StartDate AND raised_at <= @EndDate
                ORDER BY raised_at DESC
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            var rows = await connection.QueryAsync<AlertDb>(sql, new { StartDate = startDate, EndDate = endDate });
            return rows;
        }

        public async Task<IEnumerable<AlertDb>> GetAlertsByStatusDateRange(AlertStatus status, DateTime startDate, DateTime endDate)
        {
            var sql = """
                SELECT id, driver_id, event_latitude, event_longitude,
                       exit_speed, exit_time, zone_name, zone_type,
                       risk_level, assessment, recommendation, autoscale, status, raised_at
                FROM fleetpulse.alerts
                WHERE status = @Status AND raised_at >= @StartDate AND raised_at <= @EndDate
                ORDER BY raised_at DESC
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            var rows = await connection.QueryAsync<AlertDb>(sql, new { Status = status, StartDate = startDate, EndDate = endDate });
            return rows;
        }
    }
}
