
using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Contracts.Response;
using FleetPulse.Domain.Auth;
using FleetPulse.Infrastructure.Settings;
using FleetPulse.Observability.FleetMetrics;
using FleetPulse.SignalRHub.Services.Interfaces;
using Microsoft.Extensions.Options;


namespace FleetPulse.Infrastructure.Services
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
                AppMetrics.AuthenticationErrors.Inc();
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
        }
    }
}
