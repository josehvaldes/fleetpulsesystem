using FluentValidation;
using Confluent.Kafka;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Services;
using FleetPulse.SignalRHub.Validators;
using FleetPulse.SignalRHub.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;

namespace FleetPulse.SignalRHub
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddDependencies(this IServiceCollection services, ConfigurationManager config)
        {
            services.Configure<KafkaSettings>(config.GetSection(KafkaSettings.SectionName));
            services.Configure<SignalRSettings>(config.GetSection(SignalRSettings.SectionName));
            services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));
            services.Configure<AuthSettings>(config.GetSection(AuthSettings.SectionName));

            services.AddSingleton<IConsumer<string, string>>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>()
                               .GetSection(KafkaSettings.SectionName)
                               .Get<ConsumerConfig>()!;


                return new ConsumerBuilder<string, string>(config).Build();
            });

            services.AddSingleton(sp =>
            {
                var connectionString =
                    config.GetConnectionString("FleetPulseDb")!;

                return new NpgsqlDataSourceBuilder(connectionString)
                    .Build();
            });

            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            services.AddValidatorsFromAssembly(typeof(LoginRequestValidator).Assembly);

            services.AddBackgroundWorkers(config);

            services.AddCors(config);

            services.AddAppAuthentication(config);

            return services;
        }


        public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services, ConfigurationManager config) 
        {
            // AddHostedService guarantees single instance, start/stop with the host
            services.AddHostedService<GpsPingConsumer>();

            return services;
        }

        public static IServiceCollection AddCors(this IServiceCollection services, ConfigurationManager config)
        {
            var corsSettings = config.GetSection("Cors")
                     .Get<CorsSettings>() ?? new CorsSettings();

            // --- CORS for the Vite SPA ---
            services.AddCors(o => o.AddDefaultPolicy(p => p
                .WithOrigins(corsSettings.AllowedOrigins) // the VITE+React SPA runs on this port in dev mode
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials())); // SignalR requires credentials

            return services;
        }

        public static IServiceCollection AddAppAuthentication(this IServiceCollection services, ConfigurationManager config) 
        {
            var jwt = config.GetSection("JwtSettings").Get<JwtSettings>()
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
