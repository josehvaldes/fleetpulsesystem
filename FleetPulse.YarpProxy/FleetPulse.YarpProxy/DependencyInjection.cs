using FleetPulse.YarpProxy.Settings;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FleetPulse.YarpProxy
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration config)
        {
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });


            var openTelemetrySettings = config.GetSection(OpenTelemetrySettings.SectionName)
                .Get<OpenTelemetrySettings>() ?? new OpenTelemetrySettings();

            var appSettings = config.GetSection(AppSettings.SectionName)
                .Get<AppSettings>() ?? new AppSettings();

            services.AddOpenTelemetry()
                    .ConfigureResource(r => r
                        .AddService(serviceName: appSettings.AppName,
                                    serviceVersion: appSettings.AppVersion))
                    .WithTracing(tp => tp
                        .AddSource(TelemetrySettings.ActivitySourceName)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(o => o.Endpoint =
                            new Uri(openTelemetrySettings.OtlpEndpoint)));

            services.AddSingleton<TextMapPropagator>(new TraceContextPropagator());

            return services;
        }
    }
}
