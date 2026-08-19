using FleetPulse.DbWriter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Services.Interfaces
{
    public interface ICompressionService
    {
        Task<List<GpsPingDto>> ApplyTemporalCompression(IReadOnlyList<GpsPingDto> pings);
    }
}
