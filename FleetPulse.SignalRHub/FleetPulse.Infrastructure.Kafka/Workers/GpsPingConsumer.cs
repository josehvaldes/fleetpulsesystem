using Confluent.Kafka;
using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Domain.Entities;
using FleetPulse.Infrastructure.Kafka.Dtos;
using FleetPulse.Infrastructure.Kafka.Settings;
using FleetPulse.Infrastructure.Kafka.Trace;
using FleetPulse.Observability.FleetMetrics;
using FleetPulse.Observability.Traces;
using FleetPulse.SignalRHub.Infrastructure;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace FleetPulse.Infrastructure.Kafka
{
    public class GpsPingConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IRealTimeNotifier _notifier;
        private readonly ILogger<GpsPingConsumer> _logger;
        private readonly KafkaSettings _kafkaSettings;
        // Throttle: per ROADMAP — max 2Hz per driver
        private readonly Dictionary<string, DateTimeOffset> _lastSent = new();
        private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(500);
        private readonly IHealthConsumerTracker _consumerTracker;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public GpsPingConsumer([FromKeyedServices("gps-pings")] IConsumer<string, string> consumer,
                                IRealTimeNotifier notifier,
                                ILogger<GpsPingConsumer> logger,
                                IOptions<KafkaSettings> kafkaSettings,
                                IHealthConsumerTracker consumerTracker)
        {
            _kafkaSettings = kafkaSettings.Value;
            _consumer = consumer;
            _notifier = notifier;
            _logger = logger;
            _consumerTracker = consumerTracker;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting GPS ping consumer for topic '{Topic}'", _kafkaSettings.GpsPingsTopic);
            // Subscribe BEFORE entering the loop
            _consumer.Subscribe(_kafkaSettings.GpsPingsTopic);

            // Run on a thread-pool thread so we don't block app startup
            _ = Task.Run(() => ConsumeLoopAsync(stoppingToken), stoppingToken);

            return Task.CompletedTask;
        }

        private async Task ConsumeLoopAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // Blocks until a message arrives or cancellation is requested
                        var result = _consumer.Consume(stoppingToken);

                        _consumerTracker.RecordHeartbeat();

                        // Count every message consumed from Kafka, regardless of throttle
                        KafkaMetrics.GpsPingsReceived.WithLabels(_kafkaSettings.GpsPingsTopic).Inc();

                        var parentCtx = KafkaTraceContextExtractor.Extract(result.Message.Headers);
                        using var activity = Telemetry.ActivitySource.StartActivity("signalRHub.process_gps_ping", ActivityKind.Consumer, parentCtx);

                        var dto = DeserializePing(result);

                        if (dto is null)
                        {
                            _logger.LogWarning($"Received null or invalid GPS ping message from Kafka, skipping. [{result.Message.Value}]");
                            continue;
                        }

                        // Throttle per driver
                        var now = DateTimeOffset.UtcNow;
                        if (_lastSent.TryGetValue(dto.driver_id, out var last)
                            && now - last < MinInterval)
                        {
                            continue;
                        }
                        _lastSent[dto.driver_id] = now;

                        // Purge drivers not seen in the last 5 minutes, then update gauge
                        var cutoff = now - TimeSpan.FromMinutes(5);
                        foreach (var stale in _lastSent.Where(kv => kv.Value < cutoff).Select(kv => kv.Key).ToList())
                            _lastSent.Remove(stale);
                        AppMetrics.ActiveDrivers.Set(_lastSent.Count);
                        
                        // Fan-out via SignalR group (one group per fleet, or broadcast)
                        await _notifier.SendgpsPingToAllAsync(dto.Adapt<GpsPing>(), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        /* graceful shutdown */
                        break;
                    }

                    catch (ConsumeException ex)
                    {
                        _logger.LogDebug(ex, "Transient Kafka consume error. Retrying...");
                        KafkaMetrics.GpsPingErrors.WithLabels(ErrorLabel.ConsumeException.ToString(), _kafkaSettings.GpsPingsTopic).Inc();
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error in Kafka consumer loop. Retrying...");
                        KafkaMetrics.GpsPingErrors.WithLabels(ErrorLabel.UnknownError.ToString(), _kafkaSettings.GpsPingsTopic).Inc();
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                }
            }
            finally
            {
                _logger.LogInformation("Closing GPS ping consumer for topic '{Topic}'", _kafkaSettings.GpsPingsTopic);
                _consumer.Close(); // commits final offsets, leaves group cleanly
                _consumer.Dispose();
            }
        }


        private GpsPingDto? DeserializePing(ConsumeResult<string, string> result)
        {
            try
            {
                var message = result.Message.Value;
                var ping = JsonSerializer.Deserialize<GpsPingDto>(message, JsonOptions);
                return ping;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex,
                    "Failed to deserialize message at offset {Offset} on partition {Partition}",
                    result.Offset.Value, result.Partition.Value);
                return null;
            }
        }
    }
}
