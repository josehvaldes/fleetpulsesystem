using System.Diagnostics;

namespace FleetPulse.YarpProxy.Settings
{
    public class TelemetrySettings
    {
        public static string ActivitySourceName { get; } = "FleetPulse.YarpProxy";
        public static readonly ActivitySource ActivitySource =
            new(ActivitySourceName);

    }
}
