using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Infrastructure.Services;
using FleetPulse.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FleetPulse.Infrastructure
{
    public static class DependencyInjection
    {
        // Mirrors the SignalRHub validation setup (ADR 0020): this service only validates tokens, it never issues them.
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var authSettings = configuration.GetSection(AuthenticationSettings.SectionName).Get<AuthenticationSettings>()
                ?? throw new InvalidOperationException("Authentication section is missing from configuration.");

            authSettings.SchemeValidation();

            services.AddAppAuthentication(authSettings);

            services.AddScoped<IDatabaseService, DatabaseService>();

            return services;
        }

        public static IServiceCollection AddAppAuthentication(this IServiceCollection services, AuthenticationSettings authSettings)
        {
            const string SmartScheme = "Smart";

            var authenticationBuilder = services.AddAuthentication(SmartScheme);

            var localScheme = authSettings.Local.Jwt.Scheme;
            var entraScheme = authSettings.EntraId.Scheme;

            authenticationBuilder.AddPolicyScheme(SmartScheme, "Local or Entra", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var header = context.Request.Headers.Authorization.ToString();
                    if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = header["Bearer ".Length..].Trim();
                        var handler = new JwtSecurityTokenHandler();
                        if (!string.IsNullOrEmpty(token) && handler.CanReadToken(token))
                        {
                            var issuer = handler.ReadJwtToken(token).Issuer;
                            if (issuer.StartsWith("https://login.microsoftonline.com/", StringComparison.OrdinalIgnoreCase)
                                || issuer.StartsWith("https://sts.windows.net/", StringComparison.OrdinalIgnoreCase))
                                return entraScheme;
                        }
                    }
                    return localScheme;
                };
            });

            if (authSettings.Mode == AuthenticationMode.Local || authSettings.Mode == AuthenticationMode.Both)
            {
                services.AddAppLocalAuthentication(authenticationBuilder, authSettings.Local.Jwt);
            }

            if (authSettings.Mode == AuthenticationMode.EntraId || authSettings.Mode == AuthenticationMode.Both)
            {
                services.AddAppEntraIdAuthentication(authenticationBuilder, authSettings.EntraId);
            }

            services.AddAuthorizationPolicies();

            return services;
        }

        public static IServiceCollection AddAppEntraIdAuthentication(this IServiceCollection services, AuthenticationBuilder builder, EntraIdSettings entraId)
        {
            if (string.IsNullOrWhiteSpace(entraId.TenantId))
                throw new InvalidOperationException(
                    "EntraIdSettings:TenantId is not configured. " +
                    "Set it via the environment variable: Authentication__EntraId__TenantId");
            if (string.IsNullOrWhiteSpace(entraId.ClientId))
                throw new InvalidOperationException(
                     "EntraIdSettings:ClientId is not configured. " +
                     "Set it via the environment variable: Authentication__EntraId__ClientId");

            builder.AddJwtBearer(entraId.Scheme, options =>
            {
                options.Authority = $"https://login.microsoftonline.com/{entraId.TenantId}/v2.0";
                options.Audience = $"api://{entraId.ClientId}";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    // The app registration's "accessTokenAcceptedVersion" decides whether Entra issues v1 or v2 issuers.
                    ValidIssuers =
                    [
                        $"https://login.microsoftonline.com/{entraId.TenantId}/v2.0",
                        $"https://sts.windows.net/{entraId.TenantId}/"
                    ],
                    ValidAudiences = [entraId.ClientId, $"api://{entraId.ClientId}"],
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = CreateJwtEvents(onTokenValidated: context =>
                {
                    var identity = context.Principal?.Identity as ClaimsIdentity;
                    var legacyScopeClaim = identity?.FindFirst("http://schemas.microsoft.com/identity/claims/scope");

                    // Expose Entra's long scope claim type under the short "scope" name used by the policies.
                    if (identity != null && legacyScopeClaim != null)
                    {
                        identity.AddClaim(new Claim("scope", legacyScopeClaim.Value));
                    }
                    return Task.CompletedTask;
                });
            });

            return services;
        }

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("FleetUser", policy => policy.RequireClaim("scope", "access_as_user"));
                options.AddPolicy("FleetAdminOnly", policy => policy.RequireRole("Fleet.admin"));
            });

            return services;
        }

        public static IServiceCollection AddAppLocalAuthentication(this IServiceCollection services, AuthenticationBuilder builder, JwtSettings jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt.Secret))
                throw new InvalidOperationException(
                    "JwtSettings:Secret is not configured. " +
                    "Set it via the environment variable: Authentication__Local__Jwt__Secret");
            if (string.IsNullOrWhiteSpace(jwt.Issuer))
                throw new InvalidOperationException(
                    "JwtSettings:Issuer is not configured. " +
                    "Set it via the environment variable: Authentication__Local__Jwt__Issuer");
            if (string.IsNullOrWhiteSpace(jwt.Audience))
                throw new InvalidOperationException(
                    "JwtSettings:Audience is not configured. " +
                    "Set it via the environment variable: Authentication__Local__Jwt__Audience");

            builder.AddJwtBearer(jwt.Scheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.Secret)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = CreateJwtEvents();
            });

            return services;
        }

        private static JwtBearerEvents CreateJwtEvents(Func<TokenValidatedContext, Task>? onTokenValidated = null)
        {
            return new JwtBearerEvents
            {
                OnTokenValidated = onTokenValidated ?? (_ => Task.CompletedTask),
                // Surfaces the exact validation failure (issuer/audience/signature/lifetime) instead of a bare 401.
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("JwtBearerAuthentication");
                    logger.LogWarning(context.Exception, "JWT authentication failed: {Message}", context.Exception.Message);
                    return Task.CompletedTask;
                }
            };
        }
    }
}
