using FleetPulse.SignalRHub.Contracts.Requests;
using FleetPulse.SignalRHub.Contracts.Response;
using FleetPulse.SignalRHub.Hubs;
using FleetPulse.SignalRHub.Model;
using FleetPulse.SignalRHub.Services;
using FleetPulse.SignalRHub.Validators;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse.SignalRHub.Mapping
{
    public static class ApiMapping
    {
        private static readonly string version = "v1";

        public static void AddApiMapping(this WebApplication app) 
        {
            // Map the SignalR hub endpoint
            app.MapHub<FleetHub>($"/{version}/fleetHub");//.RequireAuthorization(); to protect the hub with authentication. // Update this when login page is ready

            app.MapGet("/", () => "Welcome to SignalR Hub");
            
            app.MapGet("/health", () => "Healthy");
            
            app.MapHealthChecks("/healthz");

            app.MapGet("/dbversion", async (IDatabaseService db) => await db.GetVersion(CancellationToken.None));//.RequireAuthorization(); // Update this when login page is ready

            var apiGroup = app.MapGroup($"/api/{version}");//.RequireAuthorization(); // Update this when login page is ready

            apiGroup.MapGet("/drivers", async (IDatabaseService db, [FromQuery] DateTime from) =>
            {
                var lasteststates = await db.GetLatestDriverStatesAsync(from, CancellationToken.None);
                return lasteststates.Adapt<List<LastestDriverStateResponse>>();
            });

            apiGroup.MapGet("/drivers/{id}/history", async (string id, IDatabaseService db, [FromQuery] DateTime from, [FromQuery] DateTime to) =>
            {
                var gpsHistory = await db.GetGPSHistory(id, from, to, CancellationToken.None);
                return gpsHistory.Adapt<List<GpsHistoryResponse>>();
            });//.RequireAuthorization();

            apiGroup.MapGet("/alerts", async (IDatabaseService db, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int limit = 50) =>
            {
                var alerts = await db.GetAlertsAsync(from, to, limit, CancellationToken.None);
                return alerts.Adapt<List<AlertResponse>>();
            });


            app.MapPost($"/api/{version}/login", async (IAuthService authService, 
                IValidator<LoginRequest> loginValidator, 
                [FromBody] LoginRequest request) =>
            {
                var validationResult = await loginValidator.ValidateAsync(request);
                validationResult.ThrowIfInvalid();

                var result = await authService.LoginAsync(request.Username, request.Password, CancellationToken.None);
                return result;
            });
        }
    }
}
