using Prometheus;

namespace FleetPulse.DbWriter.MetricsConfig
{
    public static class FleetMetrics
    {
        /// <summary>Total GPS pings successfully consumed from Kafka, labelled by topic.</summary>
        public static readonly Counter GpsPingsReceived = Metrics.CreateCounter(
            "fleetpulse_dbwriter_gps_pings_received_total",
            "Total GPS pings consumed from Kafka",
            new CounterConfiguration { LabelNames = ["topic"] });

        public static readonly Counter GpsPingErrors = Metrics.CreateCounter(
            "fleetpulse_dbwriter_gps_ping_errors_total",
            "Total GPS pings that failed processing",
            new CounterConfiguration { LabelNames = ["error_type", "topic"] });

        public static readonly Counter GpsPingsCompressedToDb = Metrics.CreateCounter(
            "fleetpulse_dbwriter_gps_pings_compressed_to_db_total",
            "Total GPS pings compressed and sent to TimescaleDB");

        /// <summary>Time spent flushing a batch to TimescaleDB.</summary>
        public static readonly Histogram DbFlushDuration = Metrics.CreateHistogram(
            "fleetpulse_dbwriter_db_flush_duration_seconds",
            "Time spent flushing batch to TimescaleDB",
            new HistogramConfiguration { Buckets = [.001, .005, .01, .025, .05, .1, .25, .5, 1] });

        public static readonly Counter AlertsReceived = Metrics.CreateCounter(
            "fleetpulse_dbwriter_alerts_received_total",
            "Total alerts consumed from Kafka",
            new CounterConfiguration { LabelNames = ["topic"] });

        public static readonly Counter AlertsProcessed = Metrics.CreateCounter(
            "fleetpulse_dbwriter_alerts_processed_total",
            "Total alerts successfully processed and stored in TimescaleDB");

        public static readonly Counter AlertsProcessingErrors = Metrics.CreateCounter(
            "fleetpulse_dbwriter_alerts_processing_errors_total",
            "Total alerts that failed processing",
            new CounterConfiguration { LabelNames = ["error_type", "topic"] });
    }
}
