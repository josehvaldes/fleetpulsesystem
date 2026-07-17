using System.Text.Json.Serialization;

namespace FleetPulse.SignalRHub.Contracts.Response
{
    public class GpsHistoryResponse
    {
        public string DriverId { get; init; } = string.Empty;

        public double Latitude { get; init; }

        public double Longitude { get; init; }

        public double Speed { get; set; }

        public double Heading { get; init; }

        public string Timestamp { get; init; } = string.Empty;
    }
}
