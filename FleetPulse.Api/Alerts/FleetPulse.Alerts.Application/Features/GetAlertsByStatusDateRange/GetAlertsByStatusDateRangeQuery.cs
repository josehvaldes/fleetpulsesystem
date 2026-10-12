using FleetPulse.Alerts.Domain.Entities;
using Mediator;

namespace FleetPulse.Alerts.Application.Features.GetAlertsByStatusDateRange
{
    public sealed record GetAlertsByStatusDateRangeQuery(string? Status, string? RiskLevel, DateTime? From, DateTime? To, int PageSize, int PageNumber) : IRequest<IReadOnlyList<Alert>>;
}
