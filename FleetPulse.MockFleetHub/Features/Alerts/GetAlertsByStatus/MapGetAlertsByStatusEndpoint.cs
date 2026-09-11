using FleetPulse.MockFleetHub.Contracts.Response;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse.MockFleetHub.Features.Alerts.GetAlertsByStatus
{
    public static class MapGetAlertsByStatusEndpoint
    {
        public static void MapGetAlertsByStatus(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", async (
                [FromQuery] int pagesize, 
                [FromQuery] int pagenumber,
                [FromQuery] string? status,
                [FromQuery] string? riskLevel,
                [FromQuery] DateTime? from, 
                [FromQuery] DateTime? to) => { 
                var list = new List<AlertResponse>
                {
                    new AlertResponse
                    {
                        Id = "1",
                        DriverId = "Driver1",
                        EventLatitude = 34.0522,
                        EventLongitude = -118.2437,
                        ExitSpeed = 65.0,
                        ExitTime = DateTimeOffset.UtcNow.AddMinutes(-10),
                        ZoneName = "Zone A",
                        ZoneType = "Type 1",
                        RiskLevel = riskLevel ?? "Medium",
                        Status = status ?? "New",
                        Assessment = "Assessment 1",
                        Recommendation = "Recommendation 1",
                        AutoEscalate = true,
                        RaisedAt = from.HasValue ? from.Value.AddMinutes(1) : DateTimeOffset.UtcNow,
                    },
                    new AlertResponse
                    {
                        Id = "2",
                        DriverId = "Driver2",
                        EventLatitude = 40.7128,
                        EventLongitude = -74.0060,
                        ExitSpeed = 70.0,
                        ExitTime = DateTimeOffset.UtcNow.AddMinutes(-20),
                        ZoneName = "Zone B",
                        ZoneType = "Type 2",
                        RiskLevel = riskLevel ?? "Medium",
                        Status = status ?? "New",
                        Assessment = "Assessment 2",
                        Recommendation = "Recommendation 2",
                        AutoEscalate = false,
                        RaisedAt = to.HasValue ? to.Value.AddMinutes(-1) : DateTimeOffset.UtcNow,
                    },
                    new AlertResponse
                    {
                        Id = "3",
                        DriverId = "Driver3",
                        EventLatitude = 51.5074,
                        EventLongitude = -0.1278,
                        ExitSpeed = 55.0,
                        ExitTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                        ZoneName = "Zone C",
                        ZoneType = "Type 3",
                        RiskLevel = riskLevel ?? "Medium",
                        Status = status ?? "New",
                        Assessment = "Assessment 3",
                        Recommendation = "Recommendation 3",
                        AutoEscalate = true,
                        RaisedAt = from.HasValue ? from.Value.AddMinutes(1) : DateTimeOffset.UtcNow,
                    }
                };

                return new PagedResponse<AlertResponse>(
                    Items: list.Skip((pagenumber - 1) * pagesize).Take(pagesize).ToList(),
                    TotalCount: list.Count,
                    PageNumber: pagenumber,
                    PageSize: pagesize,
                    TotalPages: (int)Math.Ceiling(list.Count / (double)pagesize),
                    HasPreviousPage: pagenumber > 1,
                    HasNextPage: pagenumber * pagesize < list.Count
                );
            }).WithName("GetAlertsByStatus")
                .WithTags("Alerts")
                .Produces<List<AlertResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
        }
    }
}
