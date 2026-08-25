using FleetPulse.MockFleetHub.Configuration;

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

ApiMapping.MapApiEndpoints(app);

app.MapControllers();

app.Run();
