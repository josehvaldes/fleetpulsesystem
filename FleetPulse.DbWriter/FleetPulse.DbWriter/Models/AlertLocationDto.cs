using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FleetPulse.DbWriter.Models
{
    public class AlertLocationDto
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }
}
