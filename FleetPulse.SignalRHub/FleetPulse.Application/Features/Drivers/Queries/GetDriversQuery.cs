using FleetPulse.Domain.Entities;
using Mediator;

namespace FleetPulse.Application.Features.Drivers.Queries
{
    public sealed record GetDriversQuery(DateTimeOffset from, DateTimeOffset? to) : IRequest<IReadOnlyList<LatestDriverState>>;
    
}
