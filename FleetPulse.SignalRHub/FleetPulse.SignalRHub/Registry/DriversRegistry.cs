using FleetPulse.Application.Features.Drivers.Queries.GetDriverHistory;
using FleetPulse.Application.Features.Drivers.Queries.GetDrivers;
using FleetPulse.Contracts.Response.Drivers;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse.SignalRHub.Registry
{
    public static class DriversRegistry
    {
        public static void RegisterDrivers(this IEndpointRouteBuilder app) 
        {
            app.MapGet("/drivers", async (IMediator mediator, [FromQuery] DateTime from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
            {
                var query = new GetDriversQuery(from, to);
                var result = await mediator.Send(query, cancellationToken);
                return result.Adapt<List<LastestDriverStateResponse>>();
            });

            app.MapGet("/drivers/{id}/history", async (IMediator mediator, string id, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken) =>
            {
                var query = new GetDriverHistoryQuery(id, from, to);
                var result = await mediator.Send(query, cancellationToken);
                return result.Adapt<List<GpsPingResponse>>();

            });
        }
    }
}
