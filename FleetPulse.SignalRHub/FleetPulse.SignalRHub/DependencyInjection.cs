using FleetPulse.Application;
using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Observability.Traces;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Services;
using FleetPulse.SignalRHub.Validators;
using FluentValidation;
using Npgsql;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FleetPulse.SignalRHub
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration config)
        {
            
            services.Configure<SignalRSettings>(config.GetSection(SignalRSettings.SectionName));

            services.Configure<OpenTelemetrySettings>(config.GetSection(OpenTelemetrySettings.SectionName));

            services.AddSingleton<IRealTimeNotifier, RealTimeNotifier>();
            
            services.AddSingleton(sp =>
            {
                var connectionString =
                    config.GetConnectionString("FleetPulseDb")!;

                return new NpgsqlDataSourceBuilder(connectionString)
                    .Build();
            });

            services.AddValidatorsFromAssembly(typeof(LoginRequestValidator).Assembly);

            services.AddCors(config);

            services.AddHealthChecks(config);

            services.AddOpenTelemetry(config);



            services.AddMediator(options => { 
                options.Assemblies = [
                    typeof(ApplicationAssemblyMarker).Assembly
                    ];
                options.ServiceLifetime = ServiceLifetime.Scoped;
            });

            return services;
        }


       

        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration config) 
        {
            services.AddHealthChecks()
                .AddNpgSql(config.GetConnectionString("FleetPulseDb")!, name: "PostgreSQL");


            return services;
        }

        public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration config)
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

        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfiguration config) 
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
