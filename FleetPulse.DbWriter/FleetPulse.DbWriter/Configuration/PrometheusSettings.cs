using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Configuration
{
    public class PrometheusSettings
    {
        public const string SectionName = "Prometheus";
        public ushort Port { get; init; } = 8080;
        public bool Enabled { get; init; } = true;
    }
}
