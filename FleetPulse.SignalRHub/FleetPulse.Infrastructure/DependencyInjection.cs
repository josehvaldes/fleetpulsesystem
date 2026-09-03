using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Infrastructure.Auth;
using FleetPulse.Infrastructure.Services;
using FleetPulse.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FleetPulse.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.Configure<AuthSettings>(configuration.GetSection(AuthSettings.SectionName));

            services.AddAppAuthentication(configuration);
           

            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }


        public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var jwt = config.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                            ?? throw new InvalidOperationException("JwtSettings section is missing from configuration.");


            if (string.IsNullOrWhiteSpace(jwt.Secret))
                throw new InvalidOperationException(
                    "JwtSettings:Secret is not configured. " +
                    "Set it via the environment variable: JwtSettings__Secret");
            if (string.IsNullOrWhiteSpace(jwt.Issuer))
                throw new InvalidOperationException(
                    "JwtSettings:Issuer is not configured. " +
                    "Set it via the environment variable: JwtSettings__Issuer");
            if (string.IsNullOrWhiteSpace(jwt.Audience))
                throw new InvalidOperationException(
                    "JwtSettings:Audience is not configured. " +
                    "Set it via the environment variable: JwtSettings__Audience");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
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
                    }
                };
            });

            services.AddAuthorizationBuilder()
                        .AddPolicy("FleetManager", policy => policy.RequireClaim("scope", "fleet:read"))
                        .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

            return services;
        }


    }
}
