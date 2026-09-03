using System.Text.Json.Serialization;

namespace FleetPulse.Infrastructure.Kafka.Dtos
{
    public class GpsPingDto
    {
        [JsonPropertyName("driver_id")]
        public string driver_id { get; init; } = string.Empty;

        [JsonPropertyName("latitude")]
        public double latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double longitude { get; init; }

        [JsonPropertyName("speed_kmh")]
        public double speed { get; set; }

        [JsonPropertyName("heading_degrees")]
        public double heading { get; init; }

        [JsonPropertyName("accuracy_meters")]
        public double accuracy { get; init; }

        [JsonPropertyName("status")]
        public string status { get; init; } = string.Empty;

        [JsonPropertyName("vehicle_type")]
        public string? vehicle_type { get; init; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset timestamp { get; init; }

        public override string ToString()
        {
            return $"DriverId: {driver_id}, Timestamp: {timestamp}, Lat: {latitude}, Lon: {longitude}, Speed: {speed}, Heading: {heading}, Accuracy: {accuracy}, Status: {status}, VehicleType: {vehicle_type}";
        }
    }
}
