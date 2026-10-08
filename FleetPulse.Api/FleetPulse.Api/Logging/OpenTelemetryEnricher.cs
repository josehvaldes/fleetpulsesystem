using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace FleetPulse.Api.Logging
{
    /// <summary>Adds W3C trace_id and span_id from the current Activity, when one is active.</summary>
    sealed class OpenTelemetryEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var activity = Activity.Current;
            if (activity is null)
                return;

            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("trace_id", activity.TraceId.ToString()));

            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("span_id", activity.SpanId.ToString()));
        }
    }
}
