using FleetPulse.MockFleetHub.Configuration;
using FleetPulse.MockFleetHub.Features.Alerts.GetAlertsByStatus;

namespace FleetPulse.MockFleetHub.Features.Alerts
{
    public static class AlertsEndpoints
    {

        public static void MapAlertEndpoints(this IEndpointRouteBuilder app, IConfiguration configuration) 
        {
            var appSettings = configuration.GetSection(AppSettings.SectionName)
                        .Get<AppSettings>() ?? new AppSettings();
            var version = appSettings.ApiVersion;

            app.Map("/", () => $"FleetPulse Mock Fleet Hub API v{version} - Alerts Endpoints");


            var group = app.MapGroup($"/api/{version}/alerts");

            group.MapGetAlertsByStatus();
        }
    }
}
