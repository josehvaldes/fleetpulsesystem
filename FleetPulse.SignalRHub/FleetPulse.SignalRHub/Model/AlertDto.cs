using Confluent.Kafka;

namespace FleetPulse.SignalRHub.Model
{
    public class AlertDto
    {
        public string Id { get; set; } = string.Empty;
        public string Driver_id { get; set; } = string.Empty;
        public string Alert_type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public DateTimeOffset Created_at { get; set; }
    }
}
