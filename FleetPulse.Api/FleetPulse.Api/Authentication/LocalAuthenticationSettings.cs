namespace FleetPulse.Api.Authentication
{
    public class LocalAuthenticationSettings
    {
        public const string SectionName = "Local";
        public JwtSettings Jwt { get; set; } = new();
    }
}
