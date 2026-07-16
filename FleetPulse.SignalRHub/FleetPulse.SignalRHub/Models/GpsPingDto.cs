using System.Text.Json.Serialization;

namespace FleetPulse.SignalRHub.Models
{
    public class GpsPingDto
    {
        public int Id { get; set; }

        [JsonPropertyName("driver_id")]
        public string DriverId { get; init; } = string.Empty;

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("speed_kmh")]
        public double Speed { get; set; }

        [JsonPropertyName("heading_degrees")]
        public double Heading { get; init; }

        [JsonPropertyName("accuracy_meters")]
        public double Accuracy { get; init; }

        [JsonPropertyName("status")]
        public string Status { get; init; } = string.Empty;

        [JsonPropertyName("vehicle_type")]
        public string? VehicleType { get; init; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; init; }

        public override string ToString()
        {
            return $"DriverId: {DriverId}, Timestamp: {Timestamp}, Lat: {Latitude}, Lon: {Longitude}, Speed: {Speed}, Heading: {Heading}, Accuracy: {Accuracy}, Status: {Status}, VehicleType: {VehicleType}";
        }
    }
}
