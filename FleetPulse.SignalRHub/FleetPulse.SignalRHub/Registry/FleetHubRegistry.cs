using FleetPulse.SignalRHub.Hubs;

namespace FleetPulse.SignalRHub.Registry
{
    public static class FleetHubRegistry
    {
        public static void RegisterHub(this IEndpointRouteBuilder app, string version) 
        {
            // Map the SignalR hub endpoint
            app.MapHub<FleetHub>($"/{version}/fleetHub").RequireAuthorization();
        }
    }
}
