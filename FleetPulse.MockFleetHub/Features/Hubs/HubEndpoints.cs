using FleetPulse.MockFleetHub.Configuration;

namespace FleetPulse.MockFleetHub.Features.Hubs
{
    public static class HubEndpoints
    {

        public static void MapHubEndpoints(this WebApplication app)
        {
            var appSettings = app.Configuration.GetSection(AppSettings.SectionName)
                        .Get<AppSettings>() ?? new AppSettings();

            var version = appSettings.ApiVersion;

            // Map the SignalR hub endpoint
            app.MapHub<FleetHub>($"/{version}/fleetHub");

        }
    }
}
