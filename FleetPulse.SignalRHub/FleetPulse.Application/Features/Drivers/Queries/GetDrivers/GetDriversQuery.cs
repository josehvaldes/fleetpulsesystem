using FleetPulse.Domain.Entities;
using Mediator;

namespace FleetPulse.Application.Features.Drivers.Queries.GetDrivers
{
    public sealed record GetDriversQuery(DateTimeOffset From, DateTimeOffset? To) : IRequest<IReadOnlyList<LatestDriverState>>;
    
}
