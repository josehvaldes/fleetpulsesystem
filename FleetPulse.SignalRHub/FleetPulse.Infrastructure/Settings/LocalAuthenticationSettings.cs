using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.Infrastructure.Settings
{
    public class LocalAuthenticationSettings
    {
        public const string SectionName = "Local";
        public JwtSettings Jwt { get; set; } = new();

        public AuthSettings Auth { get; set; } = new();
    }
}
