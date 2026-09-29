using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.Infrastructure.Settings
{
    public class AuthenticationSettings
    {
        public const string SectionName = "Authentication";
        public string Mode { get; set; } = "local"; // or "msal" or "both"

        public LocalAuthenticationSettings Local { get; set; } = new();

        public EntraIdSettings EntraId { get; set; } = new();


        public void SchemeValidation()
        {
            if (Mode == AuthenticationMode.Local || Mode == AuthenticationMode.Both)
            {
                if (string.IsNullOrWhiteSpace(Local.Jwt.Scheme))
                    throw new InvalidOperationException(
                        "Authentication:Local:Jwt:Scheme is not configured. " +
                        "Set it via the environment variable: Authentication__Local__Jwt__Scheme");
            }
            if (Mode == AuthenticationMode.EntraId || Mode == AuthenticationMode.Both)
            {
                if (string.IsNullOrWhiteSpace(EntraId.Scheme))
                    throw new InvalidOperationException(
                        "Authentication:EntraId:Scheme is not configured. " +
                        "Set it via the environment variable: Authentication__EntraId__Scheme");
            }

            if (Mode == AuthenticationMode.Both) 
            {
                if (string.Equals(Local.Jwt.Scheme, EntraId.Scheme, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        "Authentication:Local:Jwt:Scheme and Authentication:EntraId:Scheme cannot be the same when using both authentication modes. " +
                        "Set them to different values via the environment variables: Authentication__Local__Jwt__Scheme and Authentication__EntraId__Scheme");
            }
        }
    }



    public class AuthenticationMode
    {
        public const string Local = "local";
        public const string EntraId = "msal";
        public const string Both = "both";
    }
}
