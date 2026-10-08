using FleetPulse.Api.Configuration;
using FleetPulse.Application.Common.Behaviors;
using FleetPulse.Application;
using FleetPulse.Observability.Traces;
using Npgsql;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FleetPulse.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<OpenTelemetrySettings>(config.GetSection(OpenTelemetrySettings.SectionName));

            services.AddSingleton(_ =>
                new NpgsqlDataSourceBuilder(config.GetConnectionString("FleetPulseDb")!).Build());

            services.AddCorsPolicy(config);

            services.AddHealthChecks()
                .AddNpgSql(config.GetConnectionString("FleetPulseDb")!, name: "PostgreSQL");

            services.AddTelemetry(config);

            services.AddMediator(options =>
            {
                options.Assemblies = [typeof(ApplicationAssemblyMarker).Assembly];
                options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
                options.ServiceLifetime = ServiceLifetime.Scoped;
            });

            return services;
        }

        private static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config)
        {
            var corsSettings = config.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();

            services.AddCors(o => o.AddDefaultPolicy(p => p
                .WithOrigins(corsSettings.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

            return services;
        }

        private static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration config)
        {
            var openTelemetrySettings = config.GetSection(OpenTelemetrySettings.SectionName)
                .Get<OpenTelemetrySettings>() ?? new OpenTelemetrySettings();

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
                    .AddOtlpExporter(o => o.Endpoint = new Uri(openTelemetrySettings.OtlpEndpoint)));

            services.AddSingleton<TextMapPropagator>(new TraceContextPropagator());
            return services;
        }
    }
}
