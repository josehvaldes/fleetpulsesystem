using FleetPulse.Alerts.Api;
using FleetPulse.Api;
using FleetPulse.Api.Authentication;
using FleetPulse.Api.Configuration;
using FleetPulse.Api.Logging;
using FleetPulse.Api.Middleware;
using FleetPulse.Api.Observability;
using FleetPulse.Api.Registry;
using FleetPulse.Drivers.Api;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

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

builder.Logging.ClearProviders();
builder.Host.UseSerilog();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddOpenApi();

builder.Services.AddDependencies(builder.Configuration);
builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);
builder.Services.AddDriversModule(builder.Configuration);
builder.Services.AddAlertsModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.RegisterApiEndpoints();
app.AddPrometheusMapping();

app.Run();

public partial class Program { }
