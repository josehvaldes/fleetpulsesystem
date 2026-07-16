using Confluent.Kafka;
using FleetPulse.DbWriter.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FleetPulse.DbWriter.Workers
{
    internal class DbBatchWriterWorker(ILogger<DbBatchWriterWorker> logger, 
        IRedpandaConsumerService consumerService,
        ICompressionService compressionService) : BackgroundService
    {
        private const int FlushIntervalSeconds = 5;
        private const int MaxBatchSize = 1000;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
        {
   
            // Start Kafka consumption in background
            var consumeTask = consumerService.StartConsumingAsync(stoppingToken);

            // Run flush loop
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(FlushIntervalSeconds));

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await FlushBatchAsync();
            }

            // Final flush on shutdown
            await FlushBatchAsync();

            try
            {
                await consumeTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
        }

        private async Task FlushBatchAsync()
        {
            var pings = consumerService.GetBatchedPings();

            if (pings.Count == 0)
                return;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                logger.LogInformation("Flushing {Count} pings to database...", pings.Count);
                foreach (var p in pings) 
                {
                    logger.LogInformation("DriverId: {DriverId}, Timestamp: {Timestamp}, Lat: {Latitude}, Lon: {Longitude}, Speed: {SpeedKmh}, Heading: {HeadingDegrees}, Accuracy: {AccuracyMeters}, Status: {Status}, VehicleType: {VehicleType}",
                        p.DriverId, p.Timestamp, p.Latitude, p.Longitude, p.SpeedKmh, p.HeadingDegrees, p.AccuracyMeters, p.Status, p.VehicleType);
                }
                    // TODO: Phase 4.3 - Add compression logic here
                    var compressedPings = await compressionService.ApplyTemporalCompression(pings);

                    // TODO: Phase 4.4 - Add database operations here
                    // await _dbRepository.BulkInsertHistoryAsync(compressedPings);
                    // await _dbRepository.UpsertLatestStateAsync(compressedPings);

                 logger.LogInformation(
                    "Flushed {Count} pings in {ElapsedMs}ms",
                    pings.Count, stopwatch.ElapsedMilliseconds);

                consumerService.ClearBatch();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to flush batch of {Count} pings", pings.Count);
                // Don't clear buffer on failure - will retry next flush
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("DbBatchWriterWorker shutting down...");
            await base.StopAsync(cancellationToken);
        }
    }
}
