using Confluent.Kafka;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Hubs;
using FleetPulse.SignalRHub.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
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
        
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public GpsPingConsumer(IConsumer<string, string> consumer,
                                IHubContext<FleetHub> hubContext,
                                ILogger<GpsPingConsumer> logger, 
                                IOptions<KafkaSettings> kafkaSettings,
                                IOptions<SignalRSettings> signalRSettings)
        {
            _signalRSettings = signalRSettings.Value;
            _kafkaSettings = kafkaSettings.Value;
            _consumer = consumer;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Subscribe BEFORE entering the loop
            _consumer.Subscribe(_kafkaSettings.Topic);

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
                    var dto = DeserializePing(result);

                    if (dto is null) continue;

                    // Throttle per driver
                    var now = DateTimeOffset.UtcNow;
                    if (_lastSent.TryGetValue(dto.DriverId, out var last)
                        && now - last < MinInterval)
                    {
                        continue;
                    }
                    _lastSent[dto.DriverId] = now;

                    // Fan-out via SignalR group (one group per fleet, or broadcast)
                    await _hubContext.Clients.All
                        .SendAsync(_signalRSettings.CallbackMethod, dto, stoppingToken);
                }
            }
            catch (OperationCanceledException) { /* graceful shutdown */ }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
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
                var wrapper = JsonSerializer.Deserialize<MessageWrapper>(message, JsonOptions);
                if (wrapper is not null)
                {
                    var ping = JsonSerializer.Deserialize<GpsPingDto>(wrapper.Payload, JsonOptions);
                    return ping;
                }
                else
                    return null;
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
