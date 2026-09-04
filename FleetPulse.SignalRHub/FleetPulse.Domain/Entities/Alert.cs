using FleetPulse.Domain.Enums;

namespace FleetPulse.Domain.Entities
{
    /// <summary>
    /// This class follows the model of the Database to reduce the need for mapping between the two. 
    /// It is used to represent an alert in the system, containing information about the driver, event location, 
    /// speed, time, zone details, risk level, assessment, recommendation, and status.
    /// </summary>
    public class Alert
    {
        public Guid id { get; set; } = Guid.Empty;
        public string driver_id { get; set; } = string.Empty;
        public double event_latitude { get; set; }
        public double event_longitude { get; set; }
        public double exit_speed { get; set; }
        public DateTimeOffset exit_time { get; set; }
        public string zone_name { get; set; } = string.Empty;
        public string zone_type { get; set; } = string.Empty;
        public RiskLevel risk_level { get; set; } = RiskLevel.Low;
        public string assessment { get; set; } = string.Empty;
        public string recommendation { get; set; } = string.Empty;
        public bool auto_escalate { get; set; }
        public AlertStatus status { get; set; } = AlertStatus.New;
        public DateTimeOffset raised_at { get; set; }
    }
}
