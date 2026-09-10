using Confluent.Kafka;

namespace FleetPulse.DbWriter.Services.Interfaces
{
    public interface IKafkaMessageHandler
    {
        Task<bool> HandleAsync(string message, CancellationToken cancellationToken);
    }
}
