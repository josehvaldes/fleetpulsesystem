using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.MetricsConfig;
using FleetPulse.DbWriter.Services;
using FleetPulse.DbWriter.Workers;
using Npgsql;
using Prometheus;

var builder = Host.CreateApplicationBuilder(args);

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
var prometheusConfig = prometheusSection.Get<PrometheusSettings>();

// Expose /metrics on port 8080 as a standalone Kestrel endpoint.
builder.Services.AddMetricServer(options => options.Port = prometheusConfig?.Port ?? 8080);

// Accessing FleetMetrics here ensures all custom metrics are registered
// with the Prometheus registry on startup, before the first scrape.
_ = FleetMetrics.GpsPingsReceived;

var host = builder.Build();

host.Run();
