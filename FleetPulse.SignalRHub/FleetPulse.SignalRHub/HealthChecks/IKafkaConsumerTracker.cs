namespace FleetPulse.SignalRHub.HealthChecks
{
    public interface IKafkaConsumerTracker
    {
        void RecordHeartbeat();
        DateTime GetLastHeartbeat();
    }
}
