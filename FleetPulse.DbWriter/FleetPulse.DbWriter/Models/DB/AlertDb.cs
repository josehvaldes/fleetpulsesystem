using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Models.DB
{
    public class AlertDb
    {
        public string id { get; set; } = string.Empty;
        public string driver_id { get; set; } = string.Empty;
        public double event_latitude { get; set; }
        public double event_longitude { get; set; }
        public double exit_speed { get; set; }
        public DateTimeOffset exit_time { get; set; }
        public string zone_name { get; set; } = string.Empty;
        public string zone_type { get; set; } = string.Empty;
        public string risk_level { get; set; } = string.Empty;
        public string assessment { get; set; } = string.Empty;
        public string recommendation { get; set; } = string.Empty;
        public bool autoscale { get; set; }
        public DateTimeOffset raised_at { get; set; }
    }
}
