using Confluent.Kafka;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Hubs;
using FleetPulse.SignalRHub.Workers;


var builder = WebApplication.CreateBuilder(args);

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

app.UseCors();

app.MapGet("/", () => "Welcome to SignalR Hub");
app.MapHub<FleetHub>("/fleetHub");
app.Run();
