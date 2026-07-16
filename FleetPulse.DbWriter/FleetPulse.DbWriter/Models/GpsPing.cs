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
        public double SpeedKmh { get; set; }

        [JsonPropertyName("heading_degrees")]
        public double HeadingDegrees { get; init; }

        [JsonPropertyName("accuracy_meters")]
        public double AccuracyMeters { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("vehicle_type")]
        public string? VehicleType { get; init; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; init; }
    }
}
