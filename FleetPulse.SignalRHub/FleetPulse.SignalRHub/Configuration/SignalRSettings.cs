namespace FleetPulse.SignalRHub.Configuration
{
    public class SignalRSettings
    {
        public const string SectionName = "SignalR";
        public string GpsPingCallbackMethod { get; set; } = string.Empty;
        public string AlertCallbackMethod { get; set; } = string.Empty;
    }
}
