using Dapper;
using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Models.DB;
using FleetPulse.DbWriter.Services.Interfaces;
using Npgsql;

namespace FleetPulse.DbWriter.Services
{
    public class AlertDatabaseService(NpgsqlDataSource _dataSource,
        ILogger<AlertDatabaseService> _logger) : IAlertDatabaseService
    {
        public async Task<string?> AddAlert(AlertDb alert)
        {
            if (string.IsNullOrEmpty(alert.id))
                alert.id = Guid.NewGuid().ToString();

            var sql = """
                INSERT INTO alerts (
                    id, driver_id,
                    event_latitude, event_longitude,
                    exit_speed, exit_time,
                    zone_name, zone_type,
                    risk_level, assessment, recommendation,
                    autoscale, raised_at
                ) VALUES (
                    @id, @driver_id,
                    @event_latitude, @event_longitude,
                    @exit_speed, @exit_time,
                    @zone_name, @zone_type,
                    @risk_level, @assessment, @recommendation,
                    @autoscale, @raised_at
                )
                """;

            try 
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                await connection.ExecuteAsync(sql, alert);

                return alert.id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding alert to database");
                return null;
            }            
        }

        public async Task<IEnumerable<AlertDb>> GetAlertsByDriverId(string driverId)
        {
            var sql = """
                SELECT id, driver_id, event_latitude, event_longitude,
                       exit_speed, exit_time, zone_name, zone_type,
                       risk_level, assessment, recommendation, autoscale, raised_at
                FROM alerts
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
                       risk_level, assessment, recommendation, autoscale, raised_at
                FROM alerts
                WHERE raised_at >= @StartDate AND raised_at <= @EndDate
                ORDER BY raised_at DESC
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            var rows = await connection.QueryAsync<AlertDb>(sql, new { StartDate = startDate, EndDate = endDate });
            return rows;
        }

    }
}
