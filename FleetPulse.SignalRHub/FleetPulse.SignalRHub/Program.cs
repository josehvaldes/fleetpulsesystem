using FleetPulse.SignalRHub.Mapping;
using FleetPulse.SignalRHub.Middleware;
using FleetPulse.SignalRHub;

MappingConfig.RegisterMappings();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// --- SignalR ---
builder.Services.AddSignalR();

builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.AddApiMapping();
app.Run();
