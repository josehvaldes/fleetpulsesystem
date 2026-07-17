namespace FleetPulse.SignalRHub.Contracts.Response
{
    public class AlertResponse
    {
        public string Id { get; set; } = string.Empty;
        public string DriverId { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
