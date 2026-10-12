using FleetPulse.Alerts.Application.Common.Interfaces;
using FleetPulse.Alerts.Domain.Entities;
using FleetPulse.Alerts.Domain.Enums;
using Mediator;

namespace FleetPulse.Alerts.Application.Features.GetAlertsByStatusDateRange
{
    public sealed class GetAlertsByStatusDateRangeQueryHandler(IAlertsRepository repository)
        : IRequestHandler<GetAlertsByStatusDateRangeQuery, IReadOnlyList<Alert>>
    {
        public async ValueTask<IReadOnlyList<Alert>> Handle(GetAlertsByStatusDateRangeQuery request, CancellationToken cancellationToken)
        {
            AlertStatus? alertStatus = request.Status is not null ? Enum.Parse<AlertStatus>(request.Status, true) : null;
            RiskLevel? riskLevel = request.RiskLevel is not null ? Enum.Parse<RiskLevel>(request.RiskLevel, true) : null;

            var alerts = await repository.GetAlertsByStatusDateRangeAsync(
                alertStatus,
                riskLevel,
                request.From,
                request.To,
                request.PageSize,
                request.PageNumber,
                cancellationToken);

            return alerts.ToList().AsReadOnly();
        }
    }
}
