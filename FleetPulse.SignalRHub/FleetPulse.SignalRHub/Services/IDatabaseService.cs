using FleetPulse.SignalRHub.Model;

namespace FleetPulse.SignalRHub.Services
{
    public interface IDatabaseService
    {
        public Task<string> GetVersion(CancellationToken cancellationToken);
        public Task<List<LatestDriverStateDto>> GetLatestDriverStatesAsync(DateTime after, CancellationToken cancellationToken);

        public Task<List<GpsPingDto>> GetGPSHistory(string driverId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken);
        public Task<List<AlertDto>> GetAlertsAsync(DateTime startTime, DateTime endTime, int limit, CancellationToken cancellationToken);
    }
}
