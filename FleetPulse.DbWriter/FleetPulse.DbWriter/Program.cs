using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Logging;
using FleetPulse.DbWriter.MetricsConfig;
using FleetPulse.DbWriter.Services;
using FleetPulse.DbWriter.Trace;
using FleetPulse.DbWriter.Workers;
using Npgsql;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

var appSettings = builder.Configuration.GetSection(AppSettings.SectionName)
                                    .Get<AppSettings>() ?? new AppSettings();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("version", appSettings.AppVersion)
    .Enrich.WithProperty("service", appSettings.AppName)
    .Enrich.With<OpenTelemetryEnricher>()
    .WriteTo.Console(new PythonCompatibleJsonFormatter(appSettings.AppName, appSettings.AppVersion))
    .CreateLogger();

builder.Services.AddSerilog();


builder.Services.AddSingleton(sp =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("FleetPulseDb")!;

    return new NpgsqlDataSourceBuilder(connectionString)
        .Build();
});

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection(KafkaSettings.SectionName));
builder.Services.AddSingleton<ICompressionService, CompressionService>();
builder.Services.AddSingleton<IRedpandaConsumerService, RedpandaConsumerService>();
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

builder.Services.AddHostedService<DbBatchWriterWorker>();

var prometheusSection = builder.Configuration.GetSection(PrometheusSettings.SectionName);
builder.Services.Configure<PrometheusSettings>(prometheusSection);
var prometheusConfig = prometheusSection.Get<PrometheusSettings>()??new PrometheusSettings();

// Expose /metrics on port 8080 as a standalone Kestrel endpoint.
builder.Services.AddMetricServer(options => options.Port = prometheusConfig.Port);

// Accessing FleetMetrics here ensures all custom metrics are registered
// with the Prometheus registry on startup, before the first scrape.
_ = FleetMetrics.GpsPingsReceived;


// OpenTelemetry configuration
var openTelemetrySection = builder.Configuration.GetSection(OpenTelemetrySettings.SectionName);
builder.Services.Configure<OpenTelemetrySettings>(openTelemetrySection);
var openTelemetrySettings = openTelemetrySection.Get<OpenTelemetrySettings>()?? new OpenTelemetrySettings();


builder.Services.AddOpenTelemetry()
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

builder.Services.AddSingleton<TextMapPropagator>(new TraceContextPropagator());

var host = builder.Build();

host.Run();
