using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Domain.Entities;
using Mediator;

namespace FleetPulse.Application.Features.Drivers.Queries.GetDriverHistory
{
    public sealed class GetDriverHistoryQueryHandler(IDatabaseService dbService)
        : IRequestHandler<GetDriverHistoryQuery, IReadOnlyList<GpsPing>>
    {
        public async ValueTask<IReadOnlyList<GpsPing>> Handle(GetDriverHistoryQuery request, CancellationToken cancellationToken)
        {
            var history = await dbService.GetGPSHistory(request.DriverId, request.From, request.To, cancellationToken);
            return history.ToList().AsReadOnly();
        }
    }
}
