using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Application.Features.Drivers.Queries.GetDrivers;
using FleetPulse.Application.Features.Drivers.Queries.GetDriverHistory;
using FleetPulse.Contracts.Requests;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Hubs;
using FleetPulse.SignalRHub.Validators;
using FluentValidation;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using FleetPulse.Application.Features.Alerts.Queries.GetAlertsByStatusDateRange;
using FleetPulse.Application.Features.Auth.Commands.Login;
using FleetPulse.Contracts.Response.Alerts;
using FleetPulse.Contracts.Response.Drivers;
using FleetPulse.Contracts.Response;
using FleetPulse.Contracts.Response.Auth;

namespace FleetPulse.SignalRHub.Registry
{
    public static class ApiRegistry
    {
        public static void RegisterApiEndpoints(this WebApplication app)
        {
            var appSettings = app.Configuration.GetSection(AppSettings.SectionName)
                                    .Get<AppSettings>() ?? new AppSettings();

            var version = appSettings.ApiVersion;
            
            app.RegisterDefaultEndpoints();

            app.RegisterHub(version);
            
            app.RegisterAuth(version);

            var apiGroup = app.MapGroup($"/api/{version}").RequireAuthorization();

            apiGroup.RegisterDrivers();

            apiGroup.RegisterAlerts();            
        }
    }
}
