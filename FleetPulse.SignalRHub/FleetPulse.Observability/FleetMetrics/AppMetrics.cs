using Prometheus;

namespace FleetPulse.Observability.FleetMetrics
{
    /// <summary>
    /// Central registry for all custom Prometheus metrics.
    /// Metric instances must be static singletons — prometheus-net throws if you
    /// try to register the same name twice, so never create these inside a method.
    /// </summary>
    public static class AppMetrics
    {

        public static readonly Counter AlertProcessingErrors = Metrics.CreateCounter(
            "fleetpulse_signalrhub_alert_processing_errors_total",
            "Total errors encountered while processing alerts",
            new CounterConfiguration { LabelNames = ["error_type", "topic"] });

        /// <summary>Number of unique drivers seen in the last 5 minutes (sliding window in GpsPingConsumer).</summary>
        public static readonly Gauge ActiveDrivers = Metrics.CreateGauge(
            "fleetpulse_signalrhub_active_drivers",
            "Number of unique drivers seen in last 5 minutes");

        /// <summary>Current number of active SignalR WebSocket connections.</summary>
        public static readonly Gauge SignalRClients = Metrics.CreateGauge(
            "fleetpulse_signalrhub_connected_clients",
            "Current WebSocket connections");

        public static readonly Counter AuthenticationErrors = Metrics.CreateCounter(
            "fleetpulse_signalrhub_authentication_errors_total",
            "Total authentication errors encountered");

    }
}
