using Confluent.Kafka;
using System.Text.Json.Serialization;

namespace FleetPulse.SignalRHub.Model
{
    public class AlertDto
    {
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("driver_id")]
        public string DriverId { get; set; } = string.Empty;

        [JsonPropertyName("exit_location")]
        public AlertLocationDto? ExitLocation { get; set; }

        [JsonPropertyName("exit_speed")]
        public float ExitSpeed { get; set; }

        [JsonPropertyName("exit_heading")]
        public float ExitHeading { get; set; }

        [JsonPropertyName("exit_time")]
        public DateTimeOffset ExitTime { get; set; }

        [JsonPropertyName("zone_name")]
        public string ZoneName { get; set; } = string.Empty;

        [JsonPropertyName("zone_type")]
        public string ZoneType { get; set; } = string.Empty;

        [JsonPropertyName("agent_risk_level")]
        public string AgentRiskLevel { get; set; } = string.Empty;

        [JsonPropertyName("agent_assessment")]
        public string AgentAssessment { get; set; } = string.Empty;

        [JsonPropertyName("agent_recommendation")]
        public string AgentRecommendation { get; set; } = string.Empty;

        [JsonPropertyName("agent_auto_escalate")]
        public bool AgentAutoEscalate { get; set; } = false;

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
