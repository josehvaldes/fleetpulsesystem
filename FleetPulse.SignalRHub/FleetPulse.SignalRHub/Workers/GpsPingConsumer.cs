using Confluent.Kafka;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Contracts.Response;
using FleetPulse.SignalRHub.HealthChecks;
using FleetPulse.SignalRHub.Hubs;
using FleetPulse.SignalRHub.MetricsConfig;
using FleetPulse.SignalRHub.Model;
using FleetPulse.SignalRHub.Trace;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace FleetPulse.SignalRHub.Workers
{
    public class GpsPingConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IHubContext<FleetHub> _hubContext;
        private readonly ILogger<GpsPingConsumer> _logger;
        private readonly KafkaSettings _kafkaSettings;
        private readonly SignalRSettings _signalRSettings;
        // Throttle: per ROADMAP — max 2Hz per driver
        private readonly Dictionary<string, DateTimeOffset> _lastSent = new();
        private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(500);
        private readonly IKafkaConsumerTracker _consumerTracker;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public GpsPingConsumer([FromKeyedServices("gps-pings")] IConsumer<string, string> consumer,
                                IHubContext<FleetHub> hubContext,
                                ILogger<GpsPingConsumer> logger, 
                                IOptions<KafkaSettings> kafkaSettings,
                                IOptions<SignalRSettings> signalRSettings,
                                IKafkaConsumerTracker consumerTracker)
        {
            _signalRSettings = signalRSettings.Value;
            _kafkaSettings = kafkaSettings.Value;
            _consumer = consumer;
            _hubContext = hubContext;
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
                    // Blocks until a message arrives or cancellation is requested
                    var result = _consumer.Consume(stoppingToken);
                    
                    _consumerTracker.RecordHeartbeat();

                    // Count every message consumed from Kafka, regardless of throttle
                    FleetMetrics.GpsPingsReceived.WithLabels(_kafkaSettings.GpsPingsTopic).Inc();

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
                    if (_lastSent.TryGetValue(dto.Driver_Id, out var last)
                        && now - last < MinInterval)
                    {
                        continue;
                    }
                    _lastSent[dto.Driver_Id] = now;

                    // Purge drivers not seen in the last 5 minutes, then update gauge
                    var cutoff = now - TimeSpan.FromMinutes(5);
                    foreach (var stale in _lastSent.Where(kv => kv.Value < cutoff).Select(kv => kv.Key).ToList())
                        _lastSent.Remove(stale);
                    FleetMetrics.ActiveDrivers.Set(_lastSent.Count);
                    
                    //transform to AlertResponse for SignalR clients
                    var alertResponse = dto.Adapt<AlertResponse>();
                    
                    // Fan-out via SignalR group (one group per fleet, or broadcast)
                    await _hubContext.Clients.All
                        .SendAsync(_signalRSettings.GpsPingCallbackMethod, alertResponse, stoppingToken);
                }
            }
            catch (OperationCanceledException) { /* graceful shutdown */ }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Kafka consumer loop");
            }
            finally
            {
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
