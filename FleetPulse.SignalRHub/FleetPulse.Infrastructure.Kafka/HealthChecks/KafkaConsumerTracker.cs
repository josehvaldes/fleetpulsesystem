using FleetPulse.Application.Common.Interfaces;

namespace FleetPulse.Infrastructure.Kafka.HealthChecks
{
    public class KafkaConsumerTracker : IHealthConsumerTracker
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
