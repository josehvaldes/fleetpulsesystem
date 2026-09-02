using Prometheus;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.Observability.FleetMetrics
{
    public class KafkaMetrics
    {
        /// <summary>Total GPS pings successfully consumed from Kafka, labelled by topic.</summary>
        public static readonly Counter GpsPingsReceived = Metrics.CreateCounter(
            "fleetpulse_signalrhub_gps_pings_received_total",
            "Total GPS pings consumed from Kafka",
            new CounterConfiguration { LabelNames = ["topic"] });

        public static readonly Counter AlertsReceived = Metrics.CreateCounter(
            "fleetpulse_signalrhub_alerts_received_total",
            "Total alerts consumed from Kafka",
            new CounterConfiguration { LabelNames = ["topic"] });

    }
}
