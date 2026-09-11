using FleetPulse.MockFleetHub.Configuration;
using FleetPulse.MockFleetHub.Features.Drivers.GetDrivers;

namespace FleetPulse.MockFleetHub.Features.GpsPings
{
    public static class DriversEndpoints
    {

        public static void MapDriversEndpoints(this IEndpointRouteBuilder app, IConfiguration configuration)
        {
            var appSettings = configuration.GetSection(AppSettings.SectionName)
                        .Get<AppSettings>() ?? new AppSettings();
            var version = appSettings.ApiVersion;
            // Map the Drivers endpoint
            var group = app.MapGroup($"/api/{version}/drivers");
            group.MapGetDrivers();
        }

    }
}
