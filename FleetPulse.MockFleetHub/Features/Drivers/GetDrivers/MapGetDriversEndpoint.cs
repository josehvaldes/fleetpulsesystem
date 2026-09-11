using FleetPulse.MockFleetHub.Contracts.Response;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse.MockFleetHub.Features.Drivers.GetDrivers
{
    public static class MapGetDriversEndpoint
    {
        public static void MapGetDrivers(this IEndpointRouteBuilder app) 
        {
            app.MapGet("/", async ([FromQuery] DateTime from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
            {
                
                return new List<LastestDriverStateResponse>() { 
                    new LastestDriverStateResponse() {
                        DriverId = "driver1",
                        Latitude = 37.7749,
                        Longitude = -122.4194,
                        Speed = 60,
                        Heading = 90,
                        LastSeen = DateTime.UtcNow.ToString("o"),
                        Status = "Active"
                    },
                    new LastestDriverStateResponse() {
                        DriverId = "driver2",
                        Latitude = 34.0522,
                        Longitude = -118.2437,
                        Speed = 50,
                        Heading = 180,
                        LastSeen = DateTime.UtcNow.ToString("o"),
                        Status = "Inactive"
                    },
                };
            });
        }
    }
}
