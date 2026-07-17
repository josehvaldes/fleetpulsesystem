using FleetPulse.SignalRHub.Contracts.Response;
using FleetPulse.SignalRHub.Hubs;
using FleetPulse.SignalRHub.Model;
using FleetPulse.SignalRHub.Services;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse.SignalRHub.Mapping
{
    public static class ApiMapping
    {
        private static readonly string version = "v1";

        public static void AddApiMapping(this WebApplication app) 
        {
            app.MapHub<FleetHub>($"/{version}/fleetHub");
            app.MapGet("/", () => "Welcome to SignalR Hub");
            
            app.MapGet("/health", () => "Healthy");
            app.MapGet("/dbversion", async (IDatabaseService db) => await db.GetVersion(CancellationToken.None));

            app.MapGet($"/api/{version}/drivers", async (IDatabaseService db, [FromQuery] DateTime from) =>
            {
                var lasteststates = await db.GetLatestDriverStatesAsync(from, CancellationToken.None);
                return lasteststates.Adapt<List<LastestDriverStateResponse>>();
            });

            app.MapGet($"/api/{version}/drivers/{{id}}/history", async (string id, IDatabaseService db, [FromQuery] DateTime from, [FromQuery] DateTime to) =>
            {
                var gpsHistory = await db.GetGPSHistory(id, from, to, CancellationToken.None);
                return gpsHistory.Adapt<List<GpsHistoryResponse>>();
            });

            app.MapGet($"/api/{version}/alerts", async (IDatabaseService db, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int limit = 50) =>
            {
                var alerts = await db.GetAlertsAsync(from, to, limit, CancellationToken.None);
                return alerts.Adapt<List<AlertResponse>>();
            });
        }
    }
}
