using FleetPulse.Application.Features.Alerts.Queries.GetAlertsByStatusDateRange;
using FleetPulse.Contracts.Response;
using FleetPulse.Contracts.Response.Alerts;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse.SignalRHub.Registry
{
    public static class AlertsRegistry
    {
        public static void RegisterAlerts(this IEndpointRouteBuilder app) 
        {
            app.MapGet("/alerts", async (IMediator mediator,
                [FromQuery] int pagesize,
                [FromQuery] int pagenumber,
                [FromQuery] string? status,
                [FromQuery] string? riskLevel,
                [FromQuery] DateTime? from,
                [FromQuery] DateTime? to,
                CancellationToken cancellationToken) =>
            {

                var query = new GetAlertsByStatusDateRangeQuery(status, riskLevel, from, to, pagesize, pagenumber);
                var result = await mediator.Send(query, cancellationToken);
                var alerts = result.Adapt<List<AlertResponse>>();
                return new PagedResponse<AlertResponse>(
                    Items: alerts,
                    TotalCount: alerts.Count,
                    PageNumber: pagenumber,
                    PageSize: pagesize,
                    TotalPages: 1,
                    HasPreviousPage: false,
                    HasNextPage: false
                );
            });
        }
    }
}
