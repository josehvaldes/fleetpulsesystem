using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.Domain.Entities
{
    public class LatestDriverState
    {
        public string Driver_Id { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public double Heading { get; set; }
        public DateTimeOffset Last_Seen { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
