using FleetPulse.SignalRHub;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Logging;
using FleetPulse.SignalRHub.Mapping;
using FleetPulse.SignalRHub.Middleware;
using Serilog;

MappingConfig.RegisterMappings();

var builder = WebApplication.CreateBuilder(args);

var appSettings = builder.Configuration.GetSection(AppSettings.SectionName)
                                    .Get<AppSettings>() ?? new AppSettings();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("version", appSettings.AppVersion)
    .Enrich.WithProperty("service", appSettings.AppName)
    .WriteTo.Console(new PythonCompatibleJsonFormatter(appSettings.AppName, appSettings.AppVersion))
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Host.UseSerilog();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// --- SignalR ---
builder.Services.AddSignalR();

builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.AddApiMapping();
app.AddPrometheusMapping();
app.Run();

