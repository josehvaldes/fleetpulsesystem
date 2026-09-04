using FleetPulse.Domain.Entities;
using Mediator;

namespace FleetPulse.Application.Features.Alerts.Queries.GetAlertsByStatusDateRange
{
    public sealed record GetAlertsByStatusDateRangeQuery(string Status, DateTime From, DateTime To) : IRequest<IReadOnlyList<Alert>>;
}
