using FleetPulse.Domain.Entities;
using Mediator;
using System;
using System.Collections.Generic;

namespace FleetPulse.Application.Features.Drivers.Queries.GetDriverHistory
{
    public sealed record GetDriverHistoryQuery(string DriverId, DateTime From, DateTime To) : IRequest<IReadOnlyList<GpsPing>>;
}
