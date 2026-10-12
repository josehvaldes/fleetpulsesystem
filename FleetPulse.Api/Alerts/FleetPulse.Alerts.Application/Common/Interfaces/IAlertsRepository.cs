using FleetPulse.Alerts.Domain.Entities;
using FleetPulse.Alerts.Domain.Enums;

namespace FleetPulse.Alerts.Application.Common.Interfaces
{
    public interface IAlertsRepository
    {
        Task<IEnumerable<Alert>> GetAlertsByStatusDateRangeAsync(AlertStatus? status, RiskLevel? riskLevel, DateTime? startDate, DateTime? endDate, int pageSize, int pageNumber, CancellationToken cancellationToken);
    }
}
