using FleetPulse.DbWriter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Services
{
    public interface IRedpandaConsumerService : IDisposable
    {
        Task StartConsumingAsync(CancellationToken cancellationToken);
        IReadOnlyList<GpsPing> GetBatchedPings();
        void ClearBatch();
    }
}
