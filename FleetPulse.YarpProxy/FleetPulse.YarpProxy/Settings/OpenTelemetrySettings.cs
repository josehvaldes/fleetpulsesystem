namespace FleetPulse.YarpProxy.Settings
{
    public class OpenTelemetrySettings
    {
        public static string SectionName = "OpenTelemetry";

        public string OtlpEndpoint { get; set; } = "localhost:4317";
    }
}
