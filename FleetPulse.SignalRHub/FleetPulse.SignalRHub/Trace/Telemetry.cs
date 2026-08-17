using System.Diagnostics;

namespace FleetPulse.SignalRHub.Trace
{
    public static class Telemetry
    {
        public static readonly ActivitySource ActivitySource =
            new("FleetPulse");


    }
}
