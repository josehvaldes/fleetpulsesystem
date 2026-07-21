using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Contracts.Response;
using FleetPulse.SignalRHub.Model;
using Microsoft.Extensions.Options;

namespace FleetPulse.SignalRHub.Services
{
    public class AuthService(IOptions<AuthSettings> options, IJwtTokenService jwt) : IAuthService
    {
        private readonly AuthSettings _authSetting = options.Value;
        private readonly IJwtTokenService _jwtTokenService = jwt;

        public async Task<LoginResponse> LoginAsync(string username, string password, CancellationToken none)
        {
            // User/passwor feature will be implemented later. For now, we will use a default user and password from the configuration.
            if (username == _authSetting.DefaultUsername && password == _authSetting.DefaultPassword)
            {
                List<string> roles = new List<string> { "admin" };
                List<string> scopes = new List<string> { "fleet:read" };
                var authUser = new AuthUser() { Id = _authSetting.DefaultUserId, Username = _authSetting.DefaultUsername };
                var accessToken = _jwtTokenService.GenerateAccessToken(authUser, roles, scopes);
                var (refreshTokenValue, refreshTokenExpiry) = _jwtTokenService.GenerateRefreshToken();

                return new LoginResponse(accessToken, username, _jwtTokenService.AccessTokenExpirySeconds)
                {
                    RawRefreshToken = refreshTokenValue,
                    RefreshTokenExpiry = refreshTokenExpiry
                };
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
        }
    }
}
