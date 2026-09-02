
using FleetPulse.Domain.Entities;
using FleetPulse.Domain.Enums;

namespace FleetPulse.SignalRHub.Services.Interfaces
{
    public interface IDatabaseService
    {
        public Task<string> GetVersion(CancellationToken cancellationToken);
        public Task<IEnumerable<LatestDriverState>> GetLatestDriverStatesAsync(DateTime after, CancellationToken cancellationToken);

        public Task<IEnumerable<GpsPing>> GetGPSHistory(string driverId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken);
        public Task<IEnumerable<Alert>> GetAlertsAsync(DateTime startTime, DateTime endTime, int limit, CancellationToken cancellationToken);

        public Task<IEnumerable<Alert>> GetAlertsByStatusDateRangeAsync(AlertStatus status, DateTime startDate, DateTime endDate, CancellationToken ct);

    }
}
