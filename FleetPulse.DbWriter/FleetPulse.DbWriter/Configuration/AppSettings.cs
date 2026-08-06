using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Configuration
{
    public class AppSettings
    {
        public const string SectionName = "AppSettings";
        public string AppName { get; set; } = string.Empty;
        public string AppVersion { get; set; } = "1.0.0";
    }
}
