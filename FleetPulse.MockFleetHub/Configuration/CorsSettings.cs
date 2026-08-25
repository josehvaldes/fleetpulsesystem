namespace FleetPulse.MockFleetHub.Configuration
{
    public class CorsSettings
    {
        public const string SectionName = "Cors";
        public string[] AllowedOrigins { get; init; } = [];
    }
}
