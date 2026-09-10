using Confluent.Kafka;
using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.MetricsConfig;
using FleetPulse.DbWriter.Services.Interfaces;
using FleetPulse.DbWriter.Trace;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using FleetPulse.DbWriter.Infrastructure;
using FleetPulse.DbWriter.Services.Common;

namespace FleetPulse.DbWriter.Services.Alerts
{
    public class AlertConsumer(ILogger<AlertConsumer> logger,
        [FromKeyedServices("Alerts")]
        IKafkaMessageHandler alertMessageHandler,        
        IKafkaConsumerFactory kafkaConsumerFactory,
        IOptions<KafkaSettings> kafkaSettings) : KafkaConsumer(), IAlertConsumer
    {
        private IConsumer<string, string> _consumer = null!;
        private readonly KafkaSettings _settings = kafkaSettings.Value;
        private readonly KafkaLogThrottle _logThrottle = new(logger, "alerts");


        public async Task StartConsumingAsync(CancellationToken stoppingToken)
        {
            var config = CreateConsumerConfig(_settings);
            config.EnableAutoCommit = true; // Enable auto-commit for alerts, as we want to commit offsets after processing

            _consumer = kafkaConsumerFactory.Create(
                config,
                msg => LogKafkaMessage(_logThrottle, msg),
                error => _logThrottle.Emit(LogLevel.Critical, $"Kafka Error: {error.Reason}"));

            _consumer.Subscribe(_settings.AlertTopic);
            logger.LogInformation("Subscribed to Kafka topic: {Topic} with group: {GroupId}", _settings.AlertTopic, _settings.GroupId);

            try
            {
                await ConsumeLoopAsync(stoppingToken);
            }
            finally
            {
                logger.LogInformation("Closing Kafka Alert consumer for topic '{Topic}'", _settings.AlertTopic);

                _consumer.Close();
            }
        }
        private async Task ConsumeLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);

                    if (consumeResult.IsPartitionEOF)
                    {
                        logger.LogDebug("Reached end of partition {Partition}",
                            consumeResult.Partition);
                        continue;
                    }
                    FleetMetrics.AlertsReceived.WithLabels(_settings.AlertTopic).Inc();

                    var parentCtx = KafkaTraceContextExtractor.Extract(consumeResult.Message.Headers);
                    using var activity = Telemetry.ActivitySource.StartActivity("dbwriter.process_alert", ActivityKind.Consumer, parentCtx);

                    await alertMessageHandler.HandleAsync(consumeResult.Message.Value, cancellationToken);
                }
                catch (OperationCanceledException) 
                {
                    // Graceful shutdown
                    break;
                }
                catch (ConsumeException ex) 
                {
                    // handled the noise via the SetLogHandler/SetErrorHandler throttle.
                    logger.LogDebug(ex, "Alert Consume error on partition {Partition}",ex.ConsumerRecord?.Partition);
                    FleetMetrics.AlertsProcessingErrors.WithLabels(new string[] { ErrorLabel.ConsumeException.ToString(), _settings.AlertTopic }).Inc();
                    await Task.Delay(1000, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error while consuming alert");
                    FleetMetrics.AlertsProcessingErrors.WithLabels(new string[] { ErrorLabel.UnknownError.ToString(), _settings.AlertTopic }).Inc();
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }
    }
}
