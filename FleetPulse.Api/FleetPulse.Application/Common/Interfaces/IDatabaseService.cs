using FleetPulse.Domain.Entities;
using FleetPulse.Domain.Enums;

namespace FleetPulse.Application.Common.Interfaces
{
    public interface IDatabaseService
    {
        Task<string> GetVersion(CancellationToken cancellationToken);

        Task<IEnumerable<LatestDriverState>> GetLatestDriverStatesAsync(DateTime after, CancellationToken cancellationToken);

        Task<IEnumerable<GpsPing>> GetGPSHistory(string driverId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken);

        Task<IEnumerable<Alert>> GetAlertsByStatusDateRangeAsync(AlertStatus? status, RiskLevel? riskLevel, DateTime? startDate, DateTime? endDate, int pageSize, int pageNumber, CancellationToken cancellationToken);
    }
}
