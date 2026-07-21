namespace FleetPulse.SignalRHub.Contracts.Response
{
    /// <summary>
    /// Mirrors the OAuth 2.0 token endpoint response (RFC 6749 §5.1).
    /// The refresh token is delivered via an HttpOnly cookie instead of
    /// this body to prevent JavaScript access from the SPA.
    /// </summary>
    public class LoginResponse(string accessToken, string username, int expiresIn)
    {
        /// <summary>Short-lived JWT bearer token.</summary>
        public string AccessToken { get; } = accessToken;

        /// <summary>Always "Bearer" — tells the SPA how to attach the token.</summary>
        public string TokenType { get; } = "Bearer";

        /// <summary>Seconds until the access token expires.</summary>
        public int ExpiresIn { get; } = expiresIn;

        /// <summary>The authenticated user's username.</summary>
        public string Username { get; } = username;

        // Not serialized — carried internally so the controller can set the HttpOnly cookie.
        [System.Text.Json.Serialization.JsonIgnore]
        public string? RawRefreshToken { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
