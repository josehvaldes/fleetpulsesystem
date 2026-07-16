using FleetPulse.DbWriter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Services
{
    public interface ICompressionService
    {
        Task<List<GpsPing>> ApplyTemporalCompression(IReadOnlyList<GpsPing> pings);
    }
}
