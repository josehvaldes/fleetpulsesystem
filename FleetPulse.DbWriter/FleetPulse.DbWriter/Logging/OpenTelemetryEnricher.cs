using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace FleetPulse.DbWriter.Logging
{
    /// <summary>
    /// Enriches every Serilog log event with the W3C trace_id and span_id
    /// taken from the currently active <see cref="Activity"/>.
    /// The values are present only when a span is active (i.e. inside a
    /// StartActivity block), and absent otherwise — matching OTel conventions.
    /// </summary>
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
