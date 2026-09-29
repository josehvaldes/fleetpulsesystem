using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.Infrastructure.Settings
{
    public class EntraIdSettings
    {
        public const string SectionName = "EntraId";
        public string Scheme { get; init; } = "EntraIdAuth";
        public string TenantId { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;
    }
}
