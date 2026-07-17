using System.Text.Json.Serialization;

namespace FleetPulse.SignalRHub.Model
{
    public class GpsPingDto
    {
        [JsonPropertyName("driver_id")]
        public string Driver_Id { get; init; } = string.Empty;

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
            return $"DriverId: {Driver_Id}, Timestamp: {Timestamp}, Lat: {Latitude}, Lon: {Longitude}, Speed: {Speed}, Heading: {Heading}, Accuracy: {Accuracy}, Status: {Status}, VehicleType: {VehicleType}";
        }
    }
}
