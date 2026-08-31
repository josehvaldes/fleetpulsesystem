using Confluent.Kafka;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.Contracts.Response;
using FleetPulse.SignalRHub.Hubs;
using FleetPulse.SignalRHub.Infrastructure;
using FleetPulse.SignalRHub.MetricsConfig;
using FleetPulse.SignalRHub.Model;
using FleetPulse.SignalRHub.Trace;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace FleetPulse.SignalRHub.Workers
{
    public class AlertConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger<AlertConsumer> _logger;
        private readonly KafkaSettings _kafkaSettings;
        private readonly IHubContext<FleetHub> _hubContext;
        private readonly SignalRSettings _signalRSettings;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AlertConsumer([FromKeyedServices("alerts")] IConsumer<string, string> consumer,
                                ILogger<AlertConsumer> logger,
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
                    try 
                    {

                        // Blocks until a message arrives or cancellation is requested
                        var result = _consumer.Consume(stoppingToken);
                        _logger.LogInformation("Consumed Alert from Kafka topic '{Topic}': {Message}", _kafkaSettings.AlertsTopic, result.Message.Value);
                        // Count every message consumed from Kafka, regardless of throttle
                        FleetMetrics.AlertsReceived.WithLabels(_kafkaSettings.AlertsTopic).Inc();

                        var parentCtx = KafkaTraceContextExtractor.Extract(result.Message.Headers);
                        using var activity = Telemetry.ActivitySource.StartActivity("signalRHub.process_alert", ActivityKind.Consumer, parentCtx);

                        var dto = DeserializeAlert(result);

                        if (dto is null)
                        {
                            _logger.LogWarning($"Received null or invalid alert message from Kafka, skipping. [{result.Message.Value}]");
                            continue;
                        }
                        
                        //transform to AlertResponse for SignalR clients
                        var alertResponse = dto.Adapt<AlertResponse>();
                        
                        // Fan-out via SignalR group (one group per fleet, or broadcast)
                        await _hubContext.Clients.All
                            .SendAsync(_signalRSettings.AlertCallbackMethod, alertResponse, stoppingToken);
                    }
                    catch (OperationCanceledException) { 
                        /* graceful shutdown */ 
                        break;
                    }
                    catch (ConsumeException ex)
                    {
                        // Broker down, network glitch, etc.
                        // We catch it INSIDE the loop so the service doesn't die.

                        // handled the noise via the SetLogHandler/SetErrorHandler throttle.
                        _logger.LogDebug(ex, "Transient Kafka consume error. Retrying...");
                        FleetMetrics.AlertProcessingErrors.WithLabels(ErrorLabel.ConsumeException.ToString(), _kafkaSettings.AlertsTopic).Inc();

                        // Wait a moment before the next iteration so we don't spin the CPU 
                        // in a tight loop if the error is immediate.
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        // Unexpected error (e.g., DB down while processing a message)
                        _logger.LogError(ex, "Unexpected error in Kafka consumer loop. Retrying...");
                        FleetMetrics.AlertProcessingErrors.WithLabels(ErrorLabel.UnknownError.ToString(), _kafkaSettings.AlertsTopic).Inc();

                        // Wait a bit longer for unexpected errors before retrying
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                }
            }
            finally
            {
                _logger.LogInformation("Closing Alert consumer for topic '{Topic}'", _kafkaSettings.AlertsTopic);
                _consumer.Close(); // commits final offsets, leaves group cleanly
                _consumer.Dispose();
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
