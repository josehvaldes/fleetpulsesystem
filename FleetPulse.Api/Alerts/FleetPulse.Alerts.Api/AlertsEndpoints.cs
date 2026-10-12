using FleetPulse.Alerts.Application.Features.GetAlertsByStatusDateRange;
using FleetPulse.Alerts.Contracts.Response;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FleetPulse.Alerts.Api
{
    public static class AlertsEndpoints
    {
        public static IEndpointRouteBuilder MapAlertsEndpoints(this IEndpointRouteBuilder app)
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

                // Paging metadata is a stub kept identical to SignalRHub for parity; see migration_notes.md.
                return new PagedResponse<AlertResponse>(
                    Items: alerts,
                    TotalCount: alerts.Count,
                    PageNumber: pagenumber,
                    PageSize: pagesize,
                    TotalPages: 1,
                    HasPreviousPage: false,
                    HasNextPage: false);
            });

            return app;
        }
    }
}
