using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.MetricsConfig;
using FleetPulse.DbWriter.Trace;
using Hangfire;
using Hangfire.PostgreSql;
using Npgsql;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

namespace FleetPulse.DbWriter
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddOpenPrometheusDependencies(this IServiceCollection services, IConfiguration config) 
        {
            var prometheusSection = config.GetSection(PrometheusSettings.SectionName);
            services.Configure<PrometheusSettings>(prometheusSection);
            var prometheusConfig = prometheusSection.Get<PrometheusSettings>() ?? new PrometheusSettings();

            // Expose /metrics on port 8080 as a standalone Kestrel endpoint.
            services.AddMetricServer(options => options.Port = prometheusConfig.Port);

            // Accessing FleetMetrics here ensures all custom metrics are registered
            // with the Prometheus registry on startup, before the first scrape.
            _ = FleetMetrics.GpsPingsReceived;

            return services;
        }

        public static IServiceCollection AddOpenTelemetryDependencies(this IServiceCollection services, IConfiguration config) 
        {
            var appSettings = config.GetSection(AppSettings.SectionName)
                                    .Get<AppSettings>() ?? new AppSettings();
            
            // OpenTelemetry configuration
            var openTelemetrySection = config.GetSection(OpenTelemetrySettings.SectionName);
            services.Configure<OpenTelemetrySettings>(openTelemetrySection);
            var openTelemetrySettings = openTelemetrySection.Get<OpenTelemetrySettings>() ?? new OpenTelemetrySettings();


            services.AddOpenTelemetry()
                .ConfigureResource(r => r
                    .AddService(serviceName: appSettings.AppName,
                                serviceVersion: appSettings.AppVersion))
                .WithTracing(tp => tp
                    .AddSource(Telemetry.ActivitySourceName)          // DbWriter only
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddNpgsql()                               // DbWriter only — traces SQL
                    .AddOtlpExporter(o => o.Endpoint =
                        new Uri(openTelemetrySettings.OtlpEndpoint)));

            services.AddSingleton<TextMapPropagator>(new TraceContextPropagator());

            return services;
        }

        public static IServiceCollection AddHangfireConfiguration(this IServiceCollection services, IConfiguration config) 
        {
            services.AddHangfire(configuration =>
            {
                configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(options =>
                        options.UseNpgsqlConnection(config.GetConnectionString("FleetPulseDb")));
            });

            services.AddHangfireServer();
            return services;
        }
    }
}
