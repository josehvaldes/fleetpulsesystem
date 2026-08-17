using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Configuration
{
    public class OpenTelemetrySettings
    {
        public static string SectionName = "OpenTelemetry";

        public string OtlpEndpoint { get; set; } = "localhost:4317";

    }
}
