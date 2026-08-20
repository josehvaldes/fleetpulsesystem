namespace FleetPulse.SignalRHub.Model
{
    public class AlertDb
    {
        public Guid id { get; set; } = Guid.Empty;
        public string driver_id { get; set; } = string.Empty;
        public double event_latitude { get; set; }
        public double event_longitude { get; set; }
        public double exit_speed { get; set; }
        public DateTimeOffset exit_time { get; set; }
        public string zone_name { get; set; } = string.Empty;
        public string zone_type { get; set; } = string.Empty;
        public string risk_level { get; set; } = string.Empty;
        public string assessment { get; set; } = string.Empty;
        public string recommendation { get; set; } = string.Empty;
        public bool autoscale { get; set; }
        public string status { get; set; } = string.Empty;
        public DateTimeOffset raised_at { get; set; }
    }
}
