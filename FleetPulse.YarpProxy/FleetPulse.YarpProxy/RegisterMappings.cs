using Microsoft.AspNetCore.Diagnostics.HealthChecks;
namespace FleetPulse.YarpProxy
{
    public static class RegisterMappings
    {
        public static void RegisterMaps(this WebApplication app)
        {
            app.MapGet("/", () => "FleetPulse YARP Proxy is running.");

            app.MapHealthChecks("/health");
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }
    }
}
