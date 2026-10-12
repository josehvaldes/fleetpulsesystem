using Dapper;
using FleetPulse.Alerts.Application.Common.Interfaces;
using FleetPulse.Alerts.Domain.Entities;
using FleetPulse.Alerts.Domain.Enums;
using Npgsql;
using System.Data;

namespace FleetPulse.Alerts.Infrastructure.Repositories
{
    public class AlertsRepository(NpgsqlDataSource dataSource) : IAlertsRepository
    {
        public async Task<IEnumerable<Alert>> GetAlertsByStatusDateRangeAsync(AlertStatus? status, RiskLevel? riskLevel, DateTime? startDate, DateTime? endDate, int pageSize, int pageNumber, CancellationToken cancellationToken)
        {
            var sql = """
                SELECT id, driver_id, event_latitude, event_longitude,
                       exit_speed, exit_time, zone_name, zone_type,
                       risk_level, assessment, recommendation, auto_escalate, status, raised_at
                FROM alerts.alerts
                WHERE (@Status IS NULL OR status = CAST(@Status AS text))
                     AND (@RiskLevel IS NULL OR risk_level = CAST(@RiskLevel AS text))
                     AND (@StartDate IS NULL OR raised_at >= CAST(@StartDate AS timestamptz))
                     AND (@EndDate IS NULL OR raised_at <= CAST(@EndDate AS timestamptz))
                ORDER BY raised_at DESC
                LIMIT @Limit OFFSET @Offset
                """;

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            var offset = (pageNumber - 1) * pageSize;
            var parameters = new DynamicParameters();
            parameters.Add("Status", status?.ToString(), DbType.String);
            parameters.Add("RiskLevel", riskLevel?.ToString(), DbType.String);
            parameters.Add("StartDate", startDate, DbType.DateTime);
            parameters.Add("EndDate", endDate, DbType.DateTime);
            parameters.Add("Limit", pageSize);
            parameters.Add("Offset", offset);

            return await connection.QueryAsync<Alert>(sql, parameters);
        }
    }
}
