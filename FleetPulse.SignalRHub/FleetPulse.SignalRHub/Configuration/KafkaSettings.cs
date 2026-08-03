namespace FleetPulse.SignalRHub.Configuration
{
    public class KafkaSettings
    {
        public const string SectionName = "Kafka";
        public string BootstrapServers { get; init; } = string.Empty;
        public string GroupId { get; init; } = string.Empty;
        public string GpsPingsTopic { get; init; } = string.Empty;

        public string AlertsTopic { get; init; } = string.Empty;


    }
}
