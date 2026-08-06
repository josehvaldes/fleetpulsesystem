namespace FleetPulse.SignalRHub.Configuration
{
    public class AppSettings
    {
        public const string SectionName = "AppSettings";
        public string AppName { get; set; } = string.Empty;
        public string AppVersion { get; set; } = "1.0.0";
        public string ApiVersion { get; set; } = "v1";
    }
}
