using FleetPulse.Drivers.Application.Common.Interfaces;
using FleetPulse.Drivers.Domain.Entities;
using Mediator;

namespace FleetPulse.Drivers.Application.Features.GetDrivers
{
    public sealed class GetDriversQueryHandler(IDriversRepository repository)
        : IRequestHandler<GetDriversQuery, IReadOnlyList<LatestDriverState>>
    {
        public async ValueTask<IReadOnlyList<LatestDriverState>> Handle(GetDriversQuery request, CancellationToken cancellationToken)
        {
            var latestStates = await repository.GetLatestDriverStatesAsync(request.From.DateTime, cancellationToken);
            return latestStates.ToList().AsReadOnly();
        }
    }
}
