using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Configuration
{
    public class KafkaSettings
    {
        public const string SectionName = "Kafka";

        public string BootstrapServers { get; init; } = string.Empty;
        public string GroupId { get; init; } = string.Empty;
        public string Topic { get; init; } = string.Empty;

        public int HighSpeedCompressionThreshold { get; init; } = 40;
        public int HighSpeedCompressionBucketKey { get; init; } = 15;
    }
}
