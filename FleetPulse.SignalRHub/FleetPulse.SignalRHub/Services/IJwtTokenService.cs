using FleetPulse.SignalRHub.Model;

namespace FleetPulse.SignalRHub.Services
{
    public interface IJwtTokenService
    {
        /// <summary>Generates a short-lived JWT access token.</summary>
        string GenerateAccessToken(AuthUser user, IEnumerable<string> roles, IEnumerable<string>? scopes = null);

        /// <summary>Generates a cryptographically random opaque refresh token and its expiry.</summary>
        (string token, DateTime expiresAt) GenerateRefreshToken();

        /// <summary>Returns the access token lifetime in seconds (for the OAuth <c>expires_in</c> field).</summary>
        int AccessTokenExpirySeconds { get; }
    }
}
