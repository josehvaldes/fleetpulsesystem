using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Domain.Entities;
using FleetPulse.Domain.Enums;
using Mediator;

namespace FleetPulse.Application.Features.Alerts.Queries.GetAlertsByStatusDateRange
{
    public sealed class GetAlertsByStatusDateRangeQueryHandler(IDatabaseService dbService) : IRequestHandler<GetAlertsByStatusDateRangeQuery, IReadOnlyList<Alert>>
    {
        public async ValueTask<IReadOnlyList<Alert>> Handle(GetAlertsByStatusDateRangeQuery request, CancellationToken cancellationToken)
        {
            var alertStatus = Enum.Parse<AlertStatus>(request.Status, true);
            var alerts = await dbService.GetAlertsByStatusDateRangeAsync(alertStatus, request.From, request.To, cancellationToken);
            return alerts.ToList().AsReadOnly();
        }
    }
}
