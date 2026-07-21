using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FleetPulse.SignalRHub.Services
{
    public class JwtTokenService(IOptions<JwtSettings> opts) : IJwtTokenService
    {
        private readonly JwtSettings _s = opts.Value;

        public int AccessTokenExpirySeconds => _s.ExpiryMinutes * 60;

        public string GenerateAccessToken(AuthUser user, IEnumerable<string> roles, IEnumerable<string>? scopes = null)
        {
            var claims = new List<Claim>
            {
                // sub is the canonical OAuth subject — use the user's stable ID
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                // ClaimTypes.NameIdentifier maps sub for ASP.NET Identity conventions
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Name, user.Username),
                new(ClaimTypes.Name, user.Username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),

            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            if (scopes is not null)
                claims.AddRange(scopes.Select(s => new Claim("scope", s)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_s.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _s.Issuer,
                audience: _s.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_s.ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string token, DateTime expiresAt) GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(randomBytes);
            var expiresAt = DateTime.UtcNow.AddDays(_s.RefreshTokenExpiryDays);
            return (token, expiresAt);
        }
    }
}
