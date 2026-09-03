
namespace FleetPulse.Domain.Entities
{
    /// <summary>
    /// This class follows the structure of the GPS pings in the database to avoid unnecessary mapping. 
    /// It is used to represent the GPS ping data received from Kafka and sent to clients via SignalR.
    /// </summary>
    public class GpsPing
    {
        public string event_id { get; init; } = string.Empty;

        public string driver_id { get; init; } = string.Empty;

        public double latitude { get; init; }

        public double longitude { get; init; }

        public double speed { get; set; }

        public double heading { get; init; }

        public double accuracy { get; init; }

        public string status { get; init; } = string.Empty;

        public DateTimeOffset timestamp { get; init; }
    }
}
