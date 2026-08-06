using Confluent.Kafka;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.HealthChecks;
using FleetPulse.SignalRHub.Hubs;
using FleetPulse.SignalRHub.MetricsConfig;
using FleetPulse.SignalRHub.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FleetPulse.SignalRHub.Workers
{
    public class AiAlertConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger<AiAlertConsumer> _logger;
        private readonly KafkaSettings _kafkaSettings;
        private readonly IHubContext<FleetHub> _hubContext;
        private readonly SignalRSettings _signalRSettings;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AiAlertConsumer([FromKeyedServices("alerts")] IConsumer<string, string> consumer,
                                ILogger<AiAlertConsumer> logger,
                               IOptions<KafkaSettings> kafkaSettings,
                               IHubContext<FleetHub> hubContext,
                               IOptions<SignalRSettings> signalRSettings)
        {
            _consumer = consumer;
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
            _hubContext = hubContext;
            _signalRSettings = signalRSettings.Value;            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting AI alert consumer for topic '{Topic}'", _kafkaSettings.AlertsTopic);
            // Subscribe BEFORE entering the loop
            _consumer.Subscribe(_kafkaSettings.AlertsTopic);

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

                    // Count every message consumed from Kafka, regardless of throttle
                    FleetMetrics.AlertsReceived.WithLabels(_kafkaSettings.AlertsTopic).Inc();

                    var dto = DeserializeAlert(result);

                    if (dto is null)
                    {
                        _logger.LogWarning($"Received null or invalid alert message from Kafka, skipping. [{result.Message.Value}]");
                        continue;
                    }

                    // Fan-out via SignalR group (one group per fleet, or broadcast)
                    await _hubContext.Clients.All
                        .SendAsync(_signalRSettings.AlertCallbackMethod, dto, stoppingToken);
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

        private AlertDto? DeserializeAlert(ConsumeResult<string, string> result) 
        {
            try 
            { 
                var message = result.Message.Value;
                _logger.LogInformation("Received alert from Kafka: [{Message}]", message);
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
