using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Services;
using FleetPulse.DbWriter.Workers;
using Npgsql;

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

var host = builder.Build();
host.Run();
