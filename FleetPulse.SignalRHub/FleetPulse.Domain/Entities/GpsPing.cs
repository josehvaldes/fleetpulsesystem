
namespace FleetPulse.Domain.Entities
{
    public class GpsPing
    {
        public string DriverId { get; init; } = string.Empty;

        public double Latitude { get; init; }

        public double Longitude { get; init; }

        public double Speed { get; set; }

        public double Heading { get; init; }

        public double Accuracy { get; init; }

        public string Status { get; init; } = string.Empty;

        public string? VehicleType { get; init; }

        public DateTimeOffset Timestamp { get; init; }
    }
}
