using Confluent.Kafka;

namespace FleetPulse.DbWriter.Services.Interfaces
{
    public interface IKafkaConsumerFactory
    {
        IConsumer<string, string> Create(ConsumerConfig config, Action<LogMessage> logHandler, Action<Error> errorHandler);
    }
}
