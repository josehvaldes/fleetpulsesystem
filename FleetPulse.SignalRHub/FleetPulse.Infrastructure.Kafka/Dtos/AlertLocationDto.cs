using System.Text.Json.Serialization;

namespace FleetPulse.Infrastructure.Kafka.Dtos
{
    public class AlertLocationDto
    {
        [JsonPropertyName("latitude")]
        public double latitude { get; set; }
        
        [JsonPropertyName("longitude")]
        public double longitude { get; set; }
    }
}
