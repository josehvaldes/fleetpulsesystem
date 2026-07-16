using FleetPulse.DbWriter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Services
{
    public interface IDatabaseService
    {
        public Task<String> GetVersion(CancellationToken cancellationToken);
        public Task BulkInsertPingsAsync(List<GpsPing> history, CancellationToken cancellationToken);

        public Task DeletePingsForDriverAsync(string driverId, CancellationToken cancellationToken);

        public Task<List<GpsPing>> GetGpsPingsForDriverAsync(string driverId, CancellationToken cancellationToken);

        public Task UpsertLatestStateAsync(IReadOnlyList<GpsPing> pings, CancellationToken ct);

        public Task<DriverLastState?> GetDriverLastState(string driverId, CancellationToken cancellationToken);
    }
}
