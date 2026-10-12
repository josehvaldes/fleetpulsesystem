using FleetPulse.Drivers.Domain.Entities;

namespace FleetPulse.Drivers.Application.Common.Interfaces
{
    public interface IDriversRepository
    {
        Task<IEnumerable<LatestDriverState>> GetLatestDriverStatesAsync(DateTime after, CancellationToken cancellationToken);

        Task<IEnumerable<GpsPing>> GetGPSHistory(string driverId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken);
    }
}
