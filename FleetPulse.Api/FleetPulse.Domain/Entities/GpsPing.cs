namespace FleetPulse.Domain.Entities
{
    // Property names mirror the DB columns so Dapper maps without extra configuration.
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
