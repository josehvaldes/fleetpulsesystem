using System.Text.Json.Serialization;

namespace FleetPulse.SignalRHub.Model
{
    public class AlertLocationDto
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }
}
