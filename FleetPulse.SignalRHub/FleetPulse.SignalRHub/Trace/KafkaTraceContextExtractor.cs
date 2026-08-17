using Confluent.Kafka;
using System.Diagnostics;
using System.Text;
using OpenTelemetry.Context.Propagation;

namespace FleetPulse.SignalRHub.Trace
{
    public static class KafkaTraceContextExtractor
    {
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        public static ActivityContext Extract(Headers? headers)
        {
            if (headers is null) return default;

            var ctx = Propagator.Extract(default, headers, ReadHeader);
            return ctx.ActivityContext;

            static IEnumerable<string> ReadHeader(Headers h, string name)
            {
                if (h.TryGetLastBytes(name, out var bytes))
                    yield return Encoding.UTF8.GetString(bytes);
            }
        }
    }
}
