using Confluent.Kafka;
using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.MetricsConfig;
using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Services.Interfaces;
using FleetPulse.DbWriter.Trace;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using Mapster;
using FleetPulse.DbWriter.Models.DB;
using Hangfire;
using FleetPulse.DbWriter.Jobs;
using FleetPulse.DbWriter.Infrastructure;

namespace FleetPulse.DbWriter.Services
{
    public class AlertConsumer(ILogger<AlertConsumer> _logger,
        IAlertDatabaseService _alertDatabaseService,
        IOptions<KafkaSettings> kafkaSettings) : KafkaConsumer(), IAlertConsumer
    {
        private IConsumer<string, string> _consumer = null!;
        private readonly KafkaSettings _settings = kafkaSettings.Value;
        private readonly KafkaLogThrottle _logThrottle = new(_logger, "alerts");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };


        public async Task StartConsumingAsync(CancellationToken stoppingToken)
        {
            var config = CreateConsumerConfig(_settings);

            _consumer = new ConsumerBuilder<string, string>(config)
                .SetLogHandler((_, msg) => LogKafkaMessage(_logThrottle, msg))
                .SetErrorHandler((_, e) => _logThrottle.Emit(LogLevel.Critical, $"Kafka Error: {e.Reason}"))
                .Build();

            _consumer.Subscribe(_settings.AlertTopic);
            _logger.LogInformation("Subscribed to Kafka topic: {Topic} with group: {GroupId}", _settings.AlertTopic, _settings.GroupId);

            try
            {
                await ConsumeLoopAsync(stoppingToken);
            }
            finally
            {
                _logger.LogInformation("Closing Kafka Alert consumer for topic '{Topic}'", _settings.AlertTopic);

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
                        _logger.LogDebug("Reached end of partition {Partition}",
                            consumeResult.Partition);
                        continue;
                    }
                    FleetMetrics.AlertsReceived.WithLabels(_settings.AlertTopic).Inc();

                    var parentCtx = KafkaTraceContextExtractor.Extract(consumeResult.Message.Headers);
                    using var activity = Telemetry.ActivitySource.StartActivity("dbwriter.process_alert", ActivityKind.Consumer, parentCtx);

                    var alert = DeserializeAlert(consumeResult);
                    if (alert != null)
                    {
                        var alertdb = alert.Adapt<AlertDb>();
                        // don't wait for the database operation to complete, just fire and forget
                        var task = _alertDatabaseService.AddAlertAsync(alertdb, cancellationToken);

                        // Schedule the escalation job only if the risk level is high and autoscale is enabled
                        if (alertdb.risk_level == RiskLevel.High && alertdb.autoscale)
                        {
                            BackgroundJob.Schedule<EscalationJob>
                            (
                                //"escalation-alerts", No need to add queue name, as the queue is defined in the job class itself.
                                x => x.CheckAndEscalateAsync(alertdb.id, cancellationToken),
                                // Schedule the job to run after 10 seconds. Hardcoded for now, but can be made configurable later.
                                TimeSpan.FromSeconds(10)
                            );
                        }

                        // Schedule the standard alert processing job
                        BackgroundJob.Enqueue<StandardAlertJob>
                        (
                            x => x.ProcessAlertAsync(alertdb.id, cancellationToken)
                        );
                    }
                    else 
                    {
                        FleetMetrics.AlertsProcessingErrors.WithLabels(new string[] { ErrorLabel.DeserializationError.ToString(), _settings.AlertTopic }).Inc();
                    }
                }
                catch (OperationCanceledException) 
                {
                    // Graceful shutdown
                    break;
                }
                catch (ConsumeException ex) 
                {
                    // handled the noise via the SetLogHandler/SetErrorHandler throttle.
                    _logger.LogDebug(ex, "Alert Consume error on partition {Partition}",ex.ConsumerRecord?.Partition);
                    FleetMetrics.AlertsProcessingErrors.WithLabels(new string[] { ErrorLabel.ConsumeException.ToString(), _settings.AlertTopic }).Inc();
                    await Task.Delay(1000, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while consuming alert");
                    FleetMetrics.AlertsProcessingErrors.WithLabels(new string[] { ErrorLabel.UnknownError.ToString(), _settings.AlertTopic }).Inc();
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        private AlertDto? DeserializeAlert(ConsumeResult<string, string> result) 
        {
            try
            {
                var message = result.Message.Value;
                return JsonSerializer.Deserialize<AlertDto>(message, JsonOptions);
            }
            catch (JsonException)
            {
                _logger.LogWarning("Failed to deserialize message from Kafka: {Message}", result.Message.Value);
                return null;
            }
        }
    }
}
