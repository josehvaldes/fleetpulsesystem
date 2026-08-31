namespace FleetPulse.Contracts.Response
{
    public class AlertResponse
    {
        public string Id { get; set; } = string.Empty;
        public string DriverId { get; set; } = string.Empty;
        public double EventLatitude { get; set; }
        public double EventLongitude { get; set; }
        public double ExitSpeed { get; set; }
        public DateTimeOffset ExitTime { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string ZoneType { get; set; } = string.Empty;

        public string RiskLevel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Assessment { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public bool AutoScale { get; set; }
        public DateTimeOffset RaisedAt { get; set; }

    }
}
