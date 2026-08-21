using FleetPulse.SignalRHub.Model;

namespace FleetPulse.SignalRHub.Services.Interfaces
{
    public interface IDatabaseService
    {
        public Task<string> GetVersion(CancellationToken cancellationToken);
        public Task<IEnumerable<LatestDriverStateDto>> GetLatestDriverStatesAsync(DateTime after, CancellationToken cancellationToken);

        public Task<IEnumerable<GpsPingDto>> GetGPSHistory(string driverId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken);
        public Task<IEnumerable<AlertDb>> GetAlertsAsync(DateTime startTime, DateTime endTime, int limit, CancellationToken cancellationToken);

        public Task<IEnumerable<AlertDb>> GetAlertsByStatusDateRangeAsync(AlertStatus status, DateTime startDate, DateTime endDate, CancellationToken ct);

    }
}
