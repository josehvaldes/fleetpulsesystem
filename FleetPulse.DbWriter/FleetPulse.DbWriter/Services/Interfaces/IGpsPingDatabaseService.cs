using FleetPulse.DbWriter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Services.Interfaces
{
    public interface IGpsPingDatabaseService
    {
        public Task<String> GetVersion(CancellationToken cancellationToken);
        public Task BulkInsertPingsAsync(List<GpsPingDto> history, CancellationToken cancellationToken);

        public Task DeletePingsForDriverAsync(string driverId, CancellationToken cancellationToken);

        public Task<List<GpsPingDto>> GetGpsPingsForDriverAsync(string driverId, CancellationToken cancellationToken);

        public Task UpsertLatestStateAsync(IReadOnlyList<GpsPingDto> pings, CancellationToken ct);

        public Task<DriverLastState?> GetDriverLastState(string driverId, CancellationToken cancellationToken);
    }
}
