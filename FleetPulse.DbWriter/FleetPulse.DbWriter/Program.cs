using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Services;
using FleetPulse.DbWriter.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection(KafkaSettings.SectionName));

builder.Services.AddSingleton<IRedpandaConsumerService, RedpandaConsumerService>();

builder.Services.AddHostedService<DbBatchWriterWorker>();

var host = builder.Build();
host.Run();
