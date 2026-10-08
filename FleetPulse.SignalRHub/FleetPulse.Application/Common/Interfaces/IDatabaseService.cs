

namespace FleetPulse.Application.Common.Interfaces
{
    public interface IDatabaseService
    {
        public Task<string> GetVersion(CancellationToken cancellationToken);
    }
}
