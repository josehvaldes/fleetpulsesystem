using System.Diagnostics;

namespace FleetPulse.DbWriter.Trace
{
    public static class Telemetry
    {
        public static readonly ActivitySource ActivitySource =
            new("FleetPulse");
    }
}
