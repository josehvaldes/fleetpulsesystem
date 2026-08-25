using FleetPulse.MockFleetHub.Hubs;

namespace FleetPulse.MockFleetHub.Configuration
{
    public static class ApiMapping
    {

        public static void MapApiEndpoints(this WebApplication app)
        {
            var appSettings = app.Configuration.GetSection(AppSettings.SectionName)
                        .Get<AppSettings>() ?? new AppSettings();

            var version = appSettings.ApiVersion;

            // Map the SignalR hub endpoint
            app.MapHub<FleetHub>($"/{version}/fleetHub");//.RequireAuthorization(); to protect the hub with authentication. // Update this when login page is ready

        }
    }
}
