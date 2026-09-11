using FleetPulse.MockFleetHub.Configuration;
using FleetPulse.MockFleetHub.Features.Alerts;
using FleetPulse.MockFleetHub.Features.GpsPings;
using FleetPulse.MockFleetHub.Features.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddSignalR();

var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName)
         .Get<CorsSettings>() ?? new CorsSettings();

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(corsSettings.AllowedOrigins) // the VITE+React SPA runs on this port in dev mode
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials())); // SignalR requires credentials

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapHubEndpoints();
app.MapAlertEndpoints(app.Configuration);
app.MapDriversEndpoints(app.Configuration);

app.MapControllers();

app.Run();
