using Confluent.Kafka;
using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Infrastructure;
using FleetPulse.DbWriter.MetricsConfig;
using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Services.Interfaces;
using FleetPulse.DbWriter.Trace;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FleetPulse.DbWriter.Services
{
    internal class GpsPingConsumer(IOptions<KafkaSettings> settings,
            ILogger<GpsPingConsumer> _logger) : KafkaConsumer(), IGpsPingConsumer
    {
        private readonly KafkaSettings _settings = settings.Value;
        private readonly ConcurrentBag<GpsPingDto> _buffer = new();
        private readonly KafkaLogThrottle _logThrottle = new(_logger, "gps_pings");
        private IConsumer<string, string> _consumer = null!;
        private const int MaxBufferSize = 1000;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public void Dispose()
        {
            _consumer?.Dispose();
        }

        public void ClearBatch() => _buffer.Clear();

        

        public IReadOnlyList<GpsPingDto> GetBatchedPings() => _buffer.ToArray();

        public async Task StartConsumingAsync(CancellationToken cancellationToken)
        {
            var config = CreateConsumerConfig(_settings);

            _consumer = new ConsumerBuilder<string, string>(config)
                .SetLogHandler((_, msg) => LogKafkaMessage(_logThrottle, msg))
                .SetErrorHandler((_, e) =>_logThrottle.Emit(LogLevel.Critical, $"Kafka Error: {e.Reason}"))
                .Build();

            _consumer.Subscribe(_settings.GpspingTopic);
            _logger.LogInformation("Subscribed to topic '{Topic}' with group '{GroupId}'",_settings.GpspingTopic, _settings.GroupId);

            try
            {
                await ConsumeLoopAsync(cancellationToken);
            }
            finally
            {
                _logger.LogInformation("Closing Kafka GpsPing consumer for topic '{Topic}'", _settings.GpspingTopic);
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
                    
                    FleetMetrics.GpsPingsReceived.WithLabels(_settings.GpspingTopic).Inc();
                    var parentCtx = KafkaTraceContextExtractor.Extract(consumeResult.Message.Headers);

                    using var activity = Telemetry.ActivitySource.StartActivity("dbwriter.process_gps_ping", ActivityKind.Consumer, parentCtx);
                   

                    var ping = DeserializePing(consumeResult);
                    if (ping is not null)
                    {
                        _buffer.Add(ping);
                        _logger.LogTrace(
                            "Buffered ping from {Driver} at ({Lat}, {Lon}) - Buffer: {Count}",
                            ping.DriverId, ping.Latitude, ping.Longitude, _buffer.Count);
                    }
                    else 
                    {
                        FleetMetrics.GpsPingErrors.WithLabels(new string[]{ ErrorLabel.DeserializationError.ToString(), _settings.GpspingTopic }).Inc();
                    }

                    // Commit offset after successful processing
                    _consumer.Commit(consumeResult);

                    // Yield control periodically
                    await Task.Yield();
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (ConsumeException ex)
                {
                    // handled the noise via the SetLogHandler/SetErrorHandler throttle.
                    _logger.LogDebug(ex, "GpsPing Consume error on partition {Partition}",ex.ConsumerRecord?.Partition);
                    FleetMetrics.GpsPingErrors.WithLabels(new string[] { ErrorLabel.ConsumeException.ToString(), _settings.GpspingTopic }).Inc();
                    await Task.Delay(1000, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while consuming gps ping");
                    FleetMetrics.GpsPingErrors.WithLabels(new string[] { ErrorLabel.UnknownError.ToString(), _settings.GpspingTopic }).Inc();
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        private GpsPingDto? DeserializePing(ConsumeResult<string, string> result)
        {
            try
            {
                var message = result.Message.Value;
                var ping = JsonSerializer.Deserialize<GpsPingDto>(message, JsonOptions);
                if (ping is not null)
                {
                    ping.RawPayloadJson = message;
                }

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
