namespace FleetPulse.SignalRHub.HealthChecks
{
    public class KafkaConsumerTracker : IKafkaConsumerTracker
    {
        private DateTime _lastHeartbeat = DateTime.UtcNow;

        public void RecordHeartbeat()
        {
            _lastHeartbeat = DateTime.UtcNow;
        }

        public DateTime GetLastHeartbeat()
        {
            return _lastHeartbeat;
        }
    }
}
