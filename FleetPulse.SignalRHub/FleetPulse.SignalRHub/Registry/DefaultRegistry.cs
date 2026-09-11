using FleetPulse.Application.Common.Interfaces;

namespace FleetPulse.SignalRHub.Registry
{
    public static class DefaultRegistry
    {

        public static void RegisterDefaultEndpoints(this IEndpointRouteBuilder app) 
        {
            app.MapGet("/", () => "Welcome to SignalR Hub");

            app.MapGet("/health", () => "Healthy");

            app.MapHealthChecks("/healthz");

            app.MapGet("/dbversion", async (IDatabaseService db) => await db.GetVersion(CancellationToken.None)).RequireAuthorization();

        }
    }
}
