
namespace FleetPulse.Application.Common.Dtos
{
    /// <summary>
    /// Application-level authentication result used by the Application layer.
    /// Presentation layer maps this to an API contract (LoginResponse).
    /// </summary>
    public record AuthenticationResult(string AccessToken, string Username, int ExpiresIn)
    {
        // Not intended for serialization by API layer; controller may set cookie using RawRefreshToken
        public string? RawRefreshToken { get; init; }
        public DateTime RefreshTokenExpiry { get; init; }
    }
}
