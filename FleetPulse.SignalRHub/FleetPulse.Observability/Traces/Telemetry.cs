using System.Diagnostics;

namespace FleetPulse.Observability.Traces
{
    public static class Telemetry
    {
        public static string ActivitySourceName { get; } = "FleetPulse.SignalRHub";
        public static readonly ActivitySource ActivitySource =
            new(ActivitySourceName);


    }
}
