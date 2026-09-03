using System.Text.Json.Serialization;

namespace FleetPulse.Infrastructure.Kafka.Dtos
{
    public class AlertDto
    {
        public string id { get; set; } = string.Empty;

        [JsonPropertyName("driver_id")]
        public string driver_id { get; set; } = string.Empty;

        [JsonPropertyName("exit_location")]
        public AlertLocationDto exit_location { get; set; } = null!;

        [JsonPropertyName("exit_speed")]
        public float exit_speed { get; set; }

        [JsonPropertyName("exit_heading")]
        public float exit_heading { get; set; }

        [JsonPropertyName("exit_time")]
        public DateTimeOffset exit_time { get; set; }

        [JsonPropertyName("zone_name")]
        public string zone_name { get; set; } = string.Empty;

        [JsonPropertyName("zone_type")]
        public string zone_type { get; set; } = string.Empty;

        [JsonPropertyName("agent_risk_level")]
        public string agent_risk_level { get; set; } = string.Empty;

        [JsonPropertyName("agent_assessment")]
        public string agent_assessment { get; set; } = string.Empty;

        [JsonPropertyName("agent_recommendation")]
        public string agent_recommendation { get; set; } = string.Empty;

        [JsonPropertyName("agent_auto_escalate")]
        public bool agent_auto_escalate { get; set; } = false;

        [JsonPropertyName("created_at")]
        public DateTimeOffset created_at { get; set; }
    }
}
