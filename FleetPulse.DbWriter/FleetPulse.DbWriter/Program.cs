using Dapper;
using FleetPulse.DbWriter;
using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Infrastructure;
using FleetPulse.DbWriter.Jobs;
using FleetPulse.DbWriter.Logging;
using FleetPulse.DbWriter.Mappings;
using FleetPulse.DbWriter.Services;
using FleetPulse.DbWriter.Services.Interfaces;

using FleetPulse.DbWriter.Workers;
using Npgsql;
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
builder.Services.AddSingleton<IGpsPingDatabaseService, GpsPingDatabaseService>();
builder.Services.AddSingleton<ICompressionService, CompressionService>();
builder.Services.AddSingleton<IGpsPingConsumer, GpsPingConsumer>();
builder.Services.AddSingleton<IAlertDatabaseService, AlertDatabaseService>();
builder.Services.AddSingleton<IAlertConsumer, AlertConsumer>();


builder.Services.AddOpenPrometheusDependencies(builder.Configuration);
builder.Services.AddOpenTelemetryDependencies(builder.Configuration);
builder.Services.AddHangfireConfiguration(builder.Configuration);

// Register background worker for processing and writing GPS pings and Alerts to the database
builder.Services.AddHostedService<GpsPingDbBatchWriterWorker>();
builder.Services.AddHostedService<AlertWorker>();
builder.Services.AddHostedService<HangfireJobRegistrationService>();


SqlMapping.RegisterSqlMappings();

var host = builder.Build();

host.Run();
