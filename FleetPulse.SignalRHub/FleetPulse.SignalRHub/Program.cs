using Confluent.Kafka;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Mapping;
using FleetPulse.SignalRHub.Middleware;
using FleetPulse.SignalRHub.Services;
using FleetPulse.SignalRHub.Workers;
using Npgsql;

MappingConfig.RegisterMappings();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// --- SignalR ---
builder.Services.AddSignalR();
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection(KafkaSettings.SectionName));
builder.Services.Configure<SignalRSettings>(builder.Configuration.GetSection(SignalRSettings.SectionName));

builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>()
                   .GetSection(KafkaSettings.SectionName)
                   .Get<ConsumerConfig>()!;


    return new ConsumerBuilder<string, string>(config).Build();
});

builder.Services.AddSingleton(sp =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("FleetPulseDb")!;

    return new NpgsqlDataSourceBuilder(connectionString)
        .Build();
});

builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

// --- Background workers
// AddHostedService guarantees single instance, start/stop with the host
builder.Services.AddHostedService<GpsPingConsumer>(); 

// --- CORS for the Vite SPA ---
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:5173") // the VITE+React SPA runs on this port in dev mode
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials())); // SignalR requires credentials

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.AddApiMapping();
app.Run();
