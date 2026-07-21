namespace FleetPulse.SignalRHub.Configuration
{
    public class AuthSettings
    {
        public const string SectionName = "AuthSettings";
        public Guid DefaultUserId { get; set; } = Guid.Empty;
        public string DefaultUsername { get; set; } = string.Empty;
        public string DefaultPassword { get; set; } = string.Empty;
    }
}
