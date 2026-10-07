namespace FleetPulse.Api.Configuration
{
    public sealed class OpenTelemetrySettings
    {
        public const string SectionName = "OpenTelemetry";

        public string OtlpEndpoint { get; set; } = "http://localhost:4317";
    }
}
