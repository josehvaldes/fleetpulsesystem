using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Trace
{
    public class TraceContextValues
    {
        public string Traceparent { get; set; } = string.Empty;
        public string? Tracestate { get; set; }
    }
}
