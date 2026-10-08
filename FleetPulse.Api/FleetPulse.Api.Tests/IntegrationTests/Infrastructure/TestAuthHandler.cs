using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FleetPulse.Api.Tests.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Authenticates every request unless it carries "Authorization: Bearer invalid", which simulates a failed authentication.
    /// </summary>
    public class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "TestScheme";
        public const string InvalidToken = "invalid";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.Authorization.ToString().Contains(InvalidToken, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid test token."));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user"),
                new Claim(ClaimTypes.Name, "test-user"),
                new Claim("scope", "fleet:read"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
