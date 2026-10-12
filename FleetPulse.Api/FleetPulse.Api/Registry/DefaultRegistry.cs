using Npgsql;

namespace FleetPulse.Api.Registry
{
    public static class DefaultRegistry
    {
        public static void RegisterDefaultEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", () => "FleetPulse API");

            app.MapGet("/health", () => "Healthy");

            app.MapHealthChecks("/healthz");

            app.MapGet("/dbversion", async (NpgsqlDataSource dataSource, CancellationToken cancellationToken) =>
            {
                await using var command = dataSource.CreateCommand("SELECT version();");
                return await command.ExecuteScalarAsync(cancellationToken) as string ?? "Not Available";
            }).RequireAuthorization();
        }
    }
}
