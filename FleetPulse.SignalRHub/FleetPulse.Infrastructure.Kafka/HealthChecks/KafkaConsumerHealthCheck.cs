using FleetPulse.Application.Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FleetPulse.Infrastructure.Kafka.HealthChecks
{
    public class KafkaConsumerHealthCheck : IHealthCheck
    {
        private readonly IHealthConsumerTracker _tracker;
        public KafkaConsumerHealthCheck(IHealthConsumerTracker tracker)
        {
            _tracker = tracker;
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var lastHeartbeat = _tracker.GetLastHeartbeat();
            var timeSinceLastHeartbeat = DateTime.UtcNow - lastHeartbeat;
            if (timeSinceLastHeartbeat < TimeSpan.FromSeconds(30))
            {
                return Task.FromResult(HealthCheckResult.Healthy("Kafka consumer is healthy."));
            }

            // Degraded if missing for up to 2 minutes (e.g., handling a large batch or transient network lag)
            if (timeSinceLastHeartbeat < TimeSpan.FromMinutes(2))
            {
                return Task.FromResult(HealthCheckResult.Degraded($"Kafka consumer loop is sluggish. Last heartbeat: {timeSinceLastHeartbeat.TotalSeconds}s ago."));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("Kafka consumer has not sent a heartbeat in the last 30 seconds."));
            
        }
    }
}
