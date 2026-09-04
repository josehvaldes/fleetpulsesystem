using FleetPulse.Domain.Entities;
using Mediator;
using FleetPulse.Application.Common.Interfaces;

namespace FleetPulse.Application.Features.Drivers.Queries.GetDrivers
{
    public sealed class GetDriversQueryHandler(
        IDatabaseService dbService
        ) : IRequestHandler<GetDriversQuery, IReadOnlyList<LatestDriverState>>
    {
        public async ValueTask<IReadOnlyList<LatestDriverState>> Handle(GetDriversQuery request, CancellationToken cancellationToken)
        {
            var lasteststates = await dbService.GetLatestDriverStatesAsync(request.From.DateTime, cancellationToken);
            return lasteststates.ToList().AsReadOnly();
        }
    }
}
