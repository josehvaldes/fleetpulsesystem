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
            AlertStatus? alertStatus = request.Status is not null ? Enum.Parse<AlertStatus>(request.Status, true) : null;
            RiskLevel? riskLevel = request.RiskLevel is not null ? Enum.Parse<RiskLevel>(request.RiskLevel, true) : null;

            var alerts = await dbService.GetAlertsByStatusDateRangeAsync(alertStatus, 
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
