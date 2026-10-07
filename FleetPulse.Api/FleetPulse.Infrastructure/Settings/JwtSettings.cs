namespace FleetPulse.Infrastructure.Settings
{
    public class JwtSettings
    {
        public const string SectionName = "Jwt";
        public string Scheme { get; init; } = "LocalAuth";
        public string Secret { get; init; } = string.Empty;
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
    }
}
