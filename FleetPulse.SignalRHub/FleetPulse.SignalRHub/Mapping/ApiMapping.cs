using FleetPulse.SignalRHub.Configuration;
using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Contracts.Requests;
using FleetPulse.Contracts.Response;
using FleetPulse.SignalRHub.Hubs;
using FleetPulse.SignalRHub.Services.Interfaces;
using FleetPulse.SignalRHub.Validators;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using FleetPulse.Domain.Enums;

namespace FleetPulse.SignalRHub.Mapping
{
    public static class ApiMapping
    {
        

        public static void AddApiMapping(this WebApplication app) 
        {
            var appSettings = app.Configuration.GetSection(AppSettings.SectionName)
                                    .Get<AppSettings>() ?? new AppSettings();

            var version = appSettings.ApiVersion;

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
                return gpsHistory.Adapt<List<GpsPingResponse>>();
            });//.RequireAuthorization();

            apiGroup.MapGet("/alerts", async (IDatabaseService db, [FromQuery] string status, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int limit = 50) =>
            {
                var alertStatus = Enum.Parse<AlertStatus>(status, true);
                var alerts = await db.GetAlertsByStatusDateRangeAsync(alertStatus, from, to, CancellationToken.None);
                return alerts.Adapt<List<AlertResponse>>();
            });


            app.MapPost($"/api/{version}/login", async (IAuthService authService, 
                IValidator<LoginRequest> loginValidator, 
                [FromBody] LoginRequest request) =>
            {
                var validationResult = await loginValidator.ValidateAsync(request);
                validationResult.ThrowIfInvalid();

                var result = await authService.LoginAsync(request.Username, request.Password, CancellationToken.None);

                // Map application DTO to API contract
                var response = new LoginResponse(result.AccessToken, result.Username, result.ExpiresIn)
                {
                    RawRefreshToken = result.RawRefreshToken,
                    RefreshTokenExpiry = result.RefreshTokenExpiry
                };

                return response;
            });
        }
    }
}
