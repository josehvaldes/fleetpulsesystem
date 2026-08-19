using FleetPulse.DbWriter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Services.Interfaces
{
    public interface IGpsPingConsumerService : IDisposable
    {
        Task StartConsumingAsync(CancellationToken cancellationToken);
        IReadOnlyList<GpsPingDto> GetBatchedPings();
        void ClearBatch();
    }
}
