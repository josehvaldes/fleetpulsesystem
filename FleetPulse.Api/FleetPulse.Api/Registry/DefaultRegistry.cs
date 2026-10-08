using FleetPulse.Application.Common.Interfaces;

namespace FleetPulse.Api.Registry
{
    public static class DefaultRegistry
    {
        public static void RegisterDefaultEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", () => "FleetPulse API");

            app.MapGet("/health", () => "Healthy");

            app.MapHealthChecks("/healthz");

            app.MapGet("/dbversion", async (IDatabaseService db, CancellationToken cancellationToken) => await db.GetVersion(cancellationToken)).RequireAuthorization();
        }
    }
}
