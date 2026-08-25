using System.Text.Json.Serialization;

namespace FleetPulse.MockFleetHub.Contracts
{
    public class GpsPingRequest
    {
        public string DriverId { get; init; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double Speed { get; set; }

        public double Heading { get; init; }

        public double Accuracy { get; init; }

        public string Status { get; init; } = string.Empty;

        public string? VehicleType { get; init; }

        public DateTimeOffset Timestamp { get; init; }
    }
}
