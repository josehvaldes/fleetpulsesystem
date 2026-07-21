namespace FleetPulse.SignalRHub.Configuration
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";
        public string Secret { get; init; } = string.Empty;
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public int ExpiryMinutes { get; init; } = 15;
        public int RefreshTokenExpiryDays { get; init; } = 7;
        public int RefreshTokenRetentionDays { get; init; } = 30;
    }
}
