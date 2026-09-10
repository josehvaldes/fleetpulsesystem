using Confluent.Kafka;
using FleetPulse.DbWriter.Services.Interfaces;

namespace FleetPulse.DbWriter.Services
{
    public class KafkaConsumerFactory : IKafkaConsumerFactory
    {
        public IConsumer<string, string> Create(ConsumerConfig config, Action<LogMessage> logHandler, Action<Error> errorHandler)
        {
            return new ConsumerBuilder<string, string>(config)
                .SetLogHandler((_, msg) => logHandler(msg))
                .SetErrorHandler((_, error) => errorHandler(error))
                .Build();
        }
    }
}