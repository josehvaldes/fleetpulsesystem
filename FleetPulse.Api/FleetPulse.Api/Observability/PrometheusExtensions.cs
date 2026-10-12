using Microsoft.AspNetCore.Builder;
using Prometheus;

namespace FleetPulse.Api.Observability
{
    public static class PrometheusExtensions
    {
        public static void AddPrometheusMapping(this WebApplication app)
        {
            app.UseHttpMetrics();
            app.MapMetrics();
        }
    }
}
