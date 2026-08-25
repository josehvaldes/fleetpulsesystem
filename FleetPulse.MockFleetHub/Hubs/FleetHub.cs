using Microsoft.AspNetCore.SignalR;

namespace FleetPulse.MockFleetHub.Hubs
{
    public class FleetHub : Hub
    {
        public FleetHub() { }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        // Called by the CLIENT (browser) to join a specific fleet's updates
        public async Task SubscribeFleet(string fleetId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"fleet:{fleetId}");
        }

        // Called by the CLIENT (browser) to stop receiving updates
        public async Task UnsubscribeFleet(string fleetId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"fleet:{fleetId}");
        }
    }
}
