using System.Diagnostics;

namespace FleetPulse.Api.Observability
{
    public static class Telemetry
    {
        public static string ActivitySourceName { get; } = "FleetPulse.Api";
        public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    }
}
