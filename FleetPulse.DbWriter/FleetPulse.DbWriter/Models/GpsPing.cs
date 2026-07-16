using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FleetPulse.DbWriter.Models
{
    /// <summary>
    /// Message format for the GPS ping data received from Kafka:
    ///   message = {
	///     "driver_id": "string"
	///     "timestamp": "datetime - isoformat",
	///     "latitude": "float",
	///     "longitude": "float",
	///     "speed_kmh": "int",
	///     "heading_degrees": "float",
	///     "accuracy_meters": "float",
	///     "status": "string" 
	///     "vehicle_type": "string",
	///  }
    /// </summary>
    public class GpsPing
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

        [JsonIgnore]
        public string? RawPayloadJson { get; set; }

        public override string ToString()
        {
            return $"DriverId: {DriverId}, Timestamp: {Timestamp}, Lat: {Latitude}, Lon: {Longitude}, Speed: {Speed}, Heading: {Heading}, Accuracy: {Accuracy}, Status: {Status}, VehicleType: {VehicleType}";
        }
    }
}
