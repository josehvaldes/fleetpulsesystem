using FleetPulse.Drivers.Domain.Entities;
using Mediator;

namespace FleetPulse.Drivers.Application.Features.GetDrivers
{
    public sealed record GetDriversQuery(DateTimeOffset From, DateTimeOffset? To) : IRequest<IReadOnlyList<LatestDriverState>>;
}
