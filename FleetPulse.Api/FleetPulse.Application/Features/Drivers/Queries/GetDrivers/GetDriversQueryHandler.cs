using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Domain.Entities;
using Mediator;

namespace FleetPulse.Application.Features.Drivers.Queries.GetDrivers
{
    public sealed class GetDriversQueryHandler(IDatabaseService dbService)
        : IRequestHandler<GetDriversQuery, IReadOnlyList<LatestDriverState>>
    {
        public async ValueTask<IReadOnlyList<LatestDriverState>> Handle(GetDriversQuery request, CancellationToken cancellationToken)
        {
            var latestStates = await dbService.GetLatestDriverStatesAsync(request.From.DateTime, cancellationToken);
            return latestStates.ToList().AsReadOnly();
        }
    }
}
