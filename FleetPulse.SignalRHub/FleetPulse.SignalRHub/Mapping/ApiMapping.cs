using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Application.Features.Drivers.Queries.GetDrivers;
using FleetPulse.Application.Features.Drivers.Queries.GetDriverHistory;
using FleetPulse.Contracts.Requests;
using FleetPulse.Contracts.Response;
using FleetPulse.Domain.Enums;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Hubs;
using FleetPulse.SignalRHub.Validators;
using FluentValidation;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using FleetPulse.Application.Features.Alerts.Queries.GetAlertsByStatusDateRange;
using FleetPulse.Application.Features.Auth.Commands.Login;

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

            apiGroup.MapGet("/drivers", async (IMediator mediator, [FromQuery] DateTime from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
            {
                var query = new GetDriversQuery(from, to);
                var result = await mediator.Send(query, cancellationToken);
                return result.Adapt<List<LastestDriverStateResponse>>();
            });

            apiGroup.MapGet("/drivers/{id}/history", async (IMediator mediator, string id,  [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken) =>
            {
                var query = new GetDriverHistoryQuery(id, from, to);
                var result = await mediator.Send(query, cancellationToken);
                return result.Adapt<List<GpsPingResponse>>();

            });

            apiGroup.MapGet("/alerts", async (IMediator mediator, [FromQuery] string status, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int limit = 50, CancellationToken cancellationToken = default) =>
            {
                var query = new GetAlertsByStatusDateRangeQuery(status, from, to);
                var result = await mediator.Send(query, cancellationToken);
                return result.Adapt<List<AlertResponse>>();
            });


            app.MapPost($"/api/{version}/login", async (IMediator mediator, 
                IValidator<LoginRequest> loginValidator, 
                [FromBody] LoginRequest request) =>
            {
                var validationResult = await loginValidator.ValidateAsync(request);
                validationResult.ThrowIfInvalid();

                var command = new LoginCommand(request.Username, request.Password);
                var result = await mediator.Send(command);

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
