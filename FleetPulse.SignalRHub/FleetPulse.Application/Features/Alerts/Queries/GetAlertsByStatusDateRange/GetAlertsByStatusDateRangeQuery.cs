using FleetPulse.Domain.Entities;
using FleetPulse.Domain.Enums;
using Mediator;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.Application.Features.Alerts.Queries.GetAlertsByStatusDateRange
{
    public sealed record GetAlertsByStatusDateRangeQuery(string Status, DateTime From, DateTime To) : IRequest<IReadOnlyList<Alert>>;
}
