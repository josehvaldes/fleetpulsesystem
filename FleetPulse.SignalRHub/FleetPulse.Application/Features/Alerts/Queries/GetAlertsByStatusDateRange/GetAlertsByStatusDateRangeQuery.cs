using FleetPulse.Domain.Entities;
using Mediator;

namespace FleetPulse.Application.Features.Alerts.Queries.GetAlertsByStatusDateRange
{
    public sealed record GetAlertsByStatusDateRangeQuery(string? Status, string? RiskLevel, DateTime? From, DateTime? To, int PageSize, int PageNumber) : IRequest<IReadOnlyList<Alert>>;
}
