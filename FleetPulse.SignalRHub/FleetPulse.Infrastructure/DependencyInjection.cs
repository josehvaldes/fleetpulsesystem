using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Infrastructure.Auth;
using FleetPulse.Infrastructure.Services;
using FleetPulse.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FleetPulse.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var authSettings = configuration.GetSection(AuthenticationSettings.SectionName).Get<AuthenticationSettings>()
                ?? throw new InvalidOperationException("Authentication section is missing from configuration.");

            authSettings.SchemeValidation();

            var localAuthSetting = authSettings.Local ??= new LocalAuthenticationSettings();
            var localAuthenticationSectionPath = $"{AuthenticationSettings.SectionName}:{LocalAuthenticationSettings.SectionName}";
            
            services.Configure<JwtSettings>(configuration.GetSection($"{localAuthenticationSectionPath}:{JwtSettings.SectionName}"));
            services.Configure<AuthSettings>(configuration.GetSection($"{localAuthenticationSectionPath}:{AuthSettings.SectionName}"));

            services.AddAppAuthentication(authSettings);
           

            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

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
                    string? token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? header["Bearer ".Length..].Trim()
                        : context.Request.Query["access_token"].ToString(); // SignalR

                    if (!string.IsNullOrEmpty(token))
                    {
                        var handler = new JwtSecurityTokenHandler();
                        if (handler.CanReadToken(token))
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

            if(authSettings.Mode == AuthenticationMode.EntraId || authSettings.Mode == AuthenticationMode.Both)
            {
                services.AddAppEntraIdAuthentication(authenticationBuilder, authSettings.EntraId);
            }

            // Define authorization policies based on claims or roles based on AuthService.cs logic and EntraId API permissions
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
                options.Audience = $"api://{entraId.ClientId}"; //API ClientID
                //options.Audience = entraId.ClientId;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    // Accept both v1 and v2 issuer formats: the app registration's
                    // "accessTokenAcceptedVersion" manifest setting decides which one Entra ID actually issues.
                    ValidIssuers =
                    [
                        $"https://login.microsoftonline.com/{entraId.TenantId}/v2.0",
                        $"https://sts.windows.net/{entraId.TenantId}/"
                    ],
                    ValidAudiences = [entraId.ClientId, $"api://{entraId.ClientId}"],
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    ClockSkew = TimeSpan.Zero  // no tolerance on expiry
                };

                //scope mapping:
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal?.Identity as ClaimsIdentity;
                        // Find the legacy Entra ID scope claim
                        var legacyScopeClaim = identity?.FindFirst("http://schemas.microsoft.com/identity/claims/scope");

                        if (identity != null && legacyScopeClaim != null)
                        {
                            // Add a clean, short alias claim
                            identity.AddClaim(new Claim("scope", legacyScopeClaim.Value));
                        }
                        return Task.CompletedTask;
                    }
                };

                // ⚠️ SignalR-specific: 
                AddJwtEventOptionsForHub(options);
            });

            

            return services;
        }

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services) 
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("FleetUser", policy => policy.RequireClaim("scope", "access_as_user"));

                // Define a policy that requires the 'Fleet.admin' role
                options.AddPolicy("FleetAdminOnly", policy => policy.RequireRole("Fleet.admin")); // 

            });

            return services;

        }

        public static IServiceCollection AddAppLocalAuthentication(this IServiceCollection services, AuthenticationBuilder builder, JwtSettings jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt.Secret))
                throw new InvalidOperationException(
                    "JwtSettings:Secret is not configured. " +
                    "Set it via the environment variable: Authentication__JwtSettings__Secret");
            if (string.IsNullOrWhiteSpace(jwt.Issuer))
                throw new InvalidOperationException(
                    "JwtSettings:Issuer is not configured. " +
                    "Set it via the environment variable: Authentication__JwtSettings__Issuer");
            if (string.IsNullOrWhiteSpace(jwt.Audience))
                throw new InvalidOperationException(
                    "JwtSettings:Audience is not configured. " +
                    "Set it via the environment variable: Authentication__JwtSettings__Audience");

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
                    ClockSkew = TimeSpan.Zero  // no tolerance on expiry
                };
               
                // ⚠️ SignalR-specific: 
                AddJwtEventOptionsForHub(options);
            });

            return services;
        }


        private static void AddJwtEventOptionsForHub(JwtBearerOptions options)
        {
            // ⚠️ SignalR-specific: 
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/v1/fleetHub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                },
                // Surfaces the exact validation failure (issuer/audience/signature/lifetime) instead of a bare 401
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
