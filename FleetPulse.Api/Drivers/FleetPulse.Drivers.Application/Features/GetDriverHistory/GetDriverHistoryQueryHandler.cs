using FleetPulse.Drivers.Application.Common.Interfaces;
using FleetPulse.Drivers.Domain.Entities;
using Mediator;

namespace FleetPulse.Drivers.Application.Features.GetDriverHistory
{
    public sealed class GetDriverHistoryQueryHandler(IDriversRepository repository)
        : IRequestHandler<GetDriverHistoryQuery, IReadOnlyList<GpsPing>>
    {
        public async ValueTask<IReadOnlyList<GpsPing>> Handle(GetDriverHistoryQuery request, CancellationToken cancellationToken)
        {
            var history = await repository.GetGPSHistory(request.DriverId, request.From, request.To, cancellationToken);
            return history.ToList().AsReadOnly();
        }
    }
}
