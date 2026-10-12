using FleetPulse.Drivers.Application.Features.GetDriverHistory;
using FleetPulse.Drivers.Application.Features.GetDrivers;
using FleetPulse.Drivers.Contracts.Response;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FleetPulse.Drivers.Api
{
    public static class DriversEndpoints
    {
        public static IEndpointRouteBuilder MapDriversEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/drivers", async (IMediator mediator, [FromQuery] DateTime from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetDriversQuery(from, to), cancellationToken);
                return result.Adapt<List<LastestDriverStateResponse>>();
            });

            app.MapGet("/drivers/{id}/history", async (IMediator mediator, string id, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetDriverHistoryQuery(id, from, to), cancellationToken);
                return result.Adapt<List<GpsPingResponse>>();
            });

            return app;
        }
    }
}
