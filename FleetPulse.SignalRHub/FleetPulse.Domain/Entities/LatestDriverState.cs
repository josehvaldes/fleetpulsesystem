using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.Domain.Entities
{
    public class LatestDriverState
    {
        public string driver_id { get; set; } = string.Empty;
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double speed { get; set; }
        public double heading { get; set; }
        public DateTimeOffset last_seen { get; set; }
        public string status { get; set; } = string.Empty;
    }
}
