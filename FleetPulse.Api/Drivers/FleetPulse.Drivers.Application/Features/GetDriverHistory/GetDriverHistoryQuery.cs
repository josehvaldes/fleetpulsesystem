using FleetPulse.Drivers.Domain.Entities;
using Mediator;

namespace FleetPulse.Drivers.Application.Features.GetDriverHistory
{
    public sealed record GetDriverHistoryQuery(string DriverId, DateTime From, DateTime To) : IRequest<IReadOnlyList<GpsPing>>;
}
