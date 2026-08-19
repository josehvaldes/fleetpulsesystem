using Confluent.Kafka;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text;

namespace FleetPulse.DbWriter.Trace
{
    public static  class KafkaTraceContextExtractor
    {
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        public static TraceContextValues GetContext(Headers headers) 
        {
            var traceparent = headers
        .FirstOrDefault(h => h.Key == "traceparent")?
        .GetValueBytes() is byte[] traceparentBytes
            ? Encoding.UTF8.GetString(traceparentBytes)
            : null;

            var tracestate = headers
                .FirstOrDefault(h => h.Key == "tracestate")?
                .GetValueBytes() is byte[] tracestateBytes
                    ? Encoding.UTF8.GetString(tracestateBytes)
                    : null;

            return new TraceContextValues
            {
                Traceparent = traceparent?? string.Empty,
                Tracestate = tracestate
            };
        }

        public static ActivityContext Extract(Headers? headers)
        {
            if (headers is null) return default;

            var ctx = Propagator.Extract(default, headers, ReadHeader);

            // Restore baggage propagated from the producer
            Baggage.Current = ctx.Baggage;

            return ctx.ActivityContext;

            static IEnumerable<string> ReadHeader(Headers h, string name)
            {
                if (h.TryGetLastBytes(name, out var bytes))
                    yield return Encoding.UTF8.GetString(bytes);
            }
        }
    }
}
