using FleetPulse.SignalRHub.Configuration;

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

        }
    }
}
