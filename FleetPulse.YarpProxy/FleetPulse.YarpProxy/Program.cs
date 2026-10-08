using FleetPulse.YarpProxy;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();

Metrics.SuppressDefaultMetrics(new SuppressDefaultMetricOptions
{
    SuppressEventCounters = true
});

// Enable HTTP request/response metrics
app.UseHttpMetrics();

// Map the /metrics endpoint for Prometheus scraping
app.MapMetrics();

app.UseWebSockets(); 

app.MapReverseProxy();

app.RegisterMaps();

app.Run();