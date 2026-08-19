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

namespace FleetPulse.DbWriter.Services
{
    public class AlertConsumer(ILogger<AlertConsumer> _logger,
        IAlertDatabaseService _alertDatabaseService,
        IOptions<KafkaSettings> kafkaSettings) : IAlertConsumer
    {
        private IConsumer<string, string> _consumer = null!;
        private readonly KafkaSettings _settings = kafkaSettings.Value;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };


        public async Task StartConsumingAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest, // Start from the earliest message if no offset is found
                EnableAutoCommit = false,  // Manual commit for reliability
                SessionTimeoutMs = 10000,
                MaxPollIntervalMs = 300000
            };

            _consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, e) =>
                    _logger.LogError("Kafka Error: {Reason}", e.Reason))
                .SetLogHandler((_, log) =>
                {
                    if (log.Level >= SyslogLevel.Warning)
                        _logger.LogWarning("Kafka Log: {Message}", log.Message);
                })
                .Build();

            _consumer.Subscribe(_settings.AlertTopic);
            _logger.LogInformation("Subscribed to Kafka topic: {Topic} with group: {GroupId}", _settings.AlertTopic, _settings.GroupId);

            try
            {
                await ConsumeLoopAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumption cancelled gracefully");
            }
            finally
            {
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
                    if ( alert != null)
                    {
                        var alertdb = alert.Adapt<AlertDb>();
                        // don't wait for the database operation to complete, just fire and forget
                        var task = _alertDatabaseService.AddAlert(alertdb);
                    }
                }
                catch (ConsumeException ex) 
                {
                    _logger.LogError(ex, "Consume error on partition {Partition}",
                        ex.ConsumerRecord?.Partition);
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
