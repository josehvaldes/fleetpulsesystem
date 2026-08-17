using System.Diagnostics;

namespace FleetPulse.DbWriter.Trace
{
    public static class Telemetry
    {
        public static string ActivitySourceName { get; } = "FleetPulse.DbWriter";
        public static readonly ActivitySource ActivitySource =
            new(ActivitySourceName);
    }
}
