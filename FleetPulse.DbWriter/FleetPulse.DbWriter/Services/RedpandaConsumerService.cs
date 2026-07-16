using Confluent.Kafka;
using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;

namespace FleetPulse.DbWriter.Services
{
    internal class RedpandaConsumerService : IRedpandaConsumerService
    {
        private readonly KafkaSettings _settings;
        private readonly ILogger<RedpandaConsumerService> _logger;
        private readonly ConcurrentBag<GpsPing> _buffer = new();
        private IConsumer<string, string> _consumer = null!;
        
        private const int MaxBufferSize = 1000;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RedpandaConsumerService(IOptions<KafkaSettings> settings, 
            ILogger<RedpandaConsumerService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public void Dispose()
        {
            _consumer?.Dispose();
        }

        public void ClearBatch() => _buffer.Clear();

        

        public IReadOnlyList<GpsPing> GetBatchedPings() => _buffer.ToArray();

        public async Task StartConsumingAsync(CancellationToken cancellationToken)
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

            _consumer.Subscribe(_settings.Topic);
            _logger.LogInformation(
                "Subscribed to topic '{Topic}' with group '{GroupId}'",
                _settings.Topic, _settings.GroupId);

            try
            {
                await ConsumeLoopAsync(cancellationToken);
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

                    var ping = DeserializePing(consumeResult);
                    if (ping is not null)
                    {
                        _buffer.Add(ping);
                        _logger.LogTrace(
                            "Buffered ping from {Driver} at ({Lat}, {Lon}) - Buffer: {Count}",
                            ping.DriverId, ping.Latitude, ping.Longitude, _buffer.Count);
                    }

                    // Commit offset after successful processing
                    _consumer.Commit(consumeResult);

                    // Yield control periodically
                    await Task.Yield();
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Consume error on partition {Partition}",
                        ex.ConsumerRecord?.Partition);
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        private GpsPing? DeserializePing(ConsumeResult<string, string> result)
        {
            try
            {
                var message = result.Message.Value;
                var wrapper = JsonSerializer.Deserialize<MessageWrapper>(message, JsonOptions);
                if (wrapper is not null) 
                {
                    var ping = JsonSerializer.Deserialize<GpsPing>(wrapper.Payload, JsonOptions);
                    if (ping is not null)
                    {
                        ping.RawPayloadJson = wrapper.Payload;
                    }

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
