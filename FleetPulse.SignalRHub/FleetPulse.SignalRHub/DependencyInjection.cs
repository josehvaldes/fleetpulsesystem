using Confluent.Kafka;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.HealthChecks;
using FleetPulse.SignalRHub.Services;
using FleetPulse.SignalRHub.Trace;
using FleetPulse.SignalRHub.Validators;
using FleetPulse.SignalRHub.Workers;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry.Context.Propagation;
using System.Text;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FleetPulse.SignalRHub.Services.Interfaces;

namespace FleetPulse.SignalRHub
{
    public static class DependencyInjection
    {
        private static ConsumerConfig GetKafkaConfig(IServiceProvider sp) 
        {
            var kafkaConfig = sp.GetRequiredService<IConfiguration>()
                           .GetSection(KafkaSettings.SectionName)
                           .Get<ConsumerConfig>()!;

            kafkaConfig.ReconnectBackoffMs = 5000;
            kafkaConfig.ReconnectBackoffMaxMs = 60000;
            kafkaConfig.SocketConnectionSetupTimeoutMs = 10000;

            // Silence chatty librdkafka logs
            kafkaConfig.LogConnectionClose = false;
            //kafkaConfig.Debug = "none";
            kafkaConfig.LogQueue = false;

            return kafkaConfig;
        }


        public static IServiceCollection AddDependencies(this IServiceCollection services, ConfigurationManager config)
        {
            services.Configure<KafkaSettings>(config.GetSection(KafkaSettings.SectionName));
            services.Configure<SignalRSettings>(config.GetSection(SignalRSettings.SectionName));
            services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));
            services.Configure<AuthSettings>(config.GetSection(AuthSettings.SectionName));
            services.Configure<OpenTelemetrySettings>(config.GetSection(OpenTelemetrySettings.SectionName));

            services.AddKeyedSingleton<IConsumer<string, string>>("gps-pings", (sp, _) =>
            {
                var kafkaConfig = GetKafkaConfig(sp);
                IKafkaConsumerTracker tracker = sp.GetRequiredService<IKafkaConsumerTracker>();
                ILogger logger = sp.GetRequiredService<ILogger<GpsPingConsumer>>();
                var throttle = new KafkaLogThrottle(logger, "gps-pings");

                return new ConsumerBuilder<string, string>(kafkaConfig)
                    .SetStatisticsHandler((_, _) => tracker.RecordHeartbeat())
                    .SetLogHandler((_, msg) => LogKafkaMessage(throttle, msg))
                    .SetErrorHandler((_, error) =>
                        throttle.Emit(LogLevel.Critical, $"Ping [{error.Code}] {error.Reason}"))
                    .Build();
            });

            services.AddKeyedSingleton<IConsumer<string, string>>("alerts", (sp, _) =>
            {
                var kafkaConfig = GetKafkaConfig(sp);
                ILogger logger = sp.GetRequiredService<ILogger<AlertConsumer>>();
                var throttle = new KafkaLogThrottle(logger, "alerts");

                return new ConsumerBuilder<string, string>(kafkaConfig)
                    .SetLogHandler((_, msg) => LogKafkaMessage(throttle, msg))
                    .SetErrorHandler((_, error) => throttle.Emit(LogLevel.Critical, $"Alerts [{error.Code}] {error.Reason}"))
                    .Build();
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
            
            services.AddSingleton<IKafkaConsumerTracker, KafkaConsumerTracker>();

            services.AddValidatorsFromAssembly(typeof(LoginRequestValidator).Assembly);

            services.AddBackgroundWorkers(config);

            services.AddCors(config);

            services.AddAppAuthentication(config);

            services.AddHealthChecks(config);

            services.AddOpenTelemetry(config);

            return services;
        }


        public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services, ConfigurationManager config) 
        {
            // AddHostedService guarantees single instance, start/stop with the host
            services.AddHostedService<GpsPingConsumer>();
            services.AddHostedService<AlertConsumer>();

            return services;
        }

        // Maps librdkafka syslog-style levels (0=emerg..7=debug) to ILogger levels.
        private static void LogKafkaMessage(KafkaLogThrottle throttle, LogMessage msg)
        {
            // Drop the noisiest, lowest-value levels entirely during outages
            if (msg.Level is SyslogLevel.Info or SyslogLevel.Debug) return;
            var level = msg.Level switch
            {
                SyslogLevel.Emergency or SyslogLevel.Alert or SyslogLevel.Critical => LogLevel.Critical,
                SyslogLevel.Error => LogLevel.Error,
                SyslogLevel.Warning or SyslogLevel.Notice => LogLevel.Warning,
                _ => LogLevel.Trace
            };
            throttle.Emit(level, $"[{msg.Facility}] {msg.Message}");
        }

        public static IServiceCollection AddHealthChecks(this IServiceCollection services, ConfigurationManager config) 
        {
            services.AddHealthChecks()
                .AddNpgSql(config.GetConnectionString("FleetPulseDb")!, name: "PostgreSQL");
            services.AddHealthChecks()
                .AddCheck<KafkaConsumerHealthCheck>("kafka_consumer_check");

            return services;
        }

        public static IServiceCollection AddCors(this IServiceCollection services, ConfigurationManager config)
        {
            var corsSettings = config.GetSection(CorsSettings.SectionName)
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

        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, ConfigurationManager config) 
        {
            var openTelemetrySettings = config.GetSection(OpenTelemetrySettings.SectionName)
                .Get<OpenTelemetrySettings>()?? new OpenTelemetrySettings();

            var appSettings = config.GetSection(AppSettings.SectionName)
                                    .Get<AppSettings>() ?? new AppSettings();


            services.AddOpenTelemetry()
                .ConfigureResource(r => r
                    .AddService(serviceName: appSettings.AppName,
                                serviceVersion: appSettings.AppVersion))
                .WithTracing(tp => tp
                    .AddSource(Telemetry.ActivitySourceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(o => o.Endpoint =
                        new Uri(openTelemetrySettings.OtlpEndpoint)));

            services.AddSingleton<TextMapPropagator>(new TraceContextPropagator());
            return services;
        }
    }
}
