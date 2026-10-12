using FleetPulse.Alerts.Api;
using FleetPulse.Api.Configuration;
using FleetPulse.Drivers.Api;

namespace FleetPulse.Api.Registry
{
    public static class ApiRegistry
    {
        public static void RegisterApiEndpoints(this WebApplication app)
        {
            var appSettings = app.Configuration.GetSection(AppSettings.SectionName)
                                    .Get<AppSettings>() ?? new AppSettings();

            app.RegisterDefaultEndpoints();

            var apiGroup = app.MapGroup($"/api/{appSettings.ApiVersion}").RequireAuthorization();

            apiGroup.MapDriversEndpoints();

            apiGroup.MapAlertsEndpoints();
        }
    }
}
