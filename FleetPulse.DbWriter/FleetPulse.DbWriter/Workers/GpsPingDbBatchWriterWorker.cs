using FleetPulse.DbWriter.MetricsConfig;
using FleetPulse.DbWriter.Services.Interfaces;
using FleetPulse.DbWriter.Trace;
using System.Diagnostics;

namespace FleetPulse.DbWriter.Workers
{
    internal class GpsPingDbBatchWriterWorker(ILogger<GpsPingDbBatchWriterWorker> logger, 
        IGpsPingConsumer consumerService,
        ICompressionService compressionService,
        IGpsPingDatabaseService databaseService) : BackgroundService
    {
        private const int FlushIntervalSeconds = 5;
        private const int MaxBatchSize = 1000;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
        {
            logger.LogInformation("DbBatchWriterWorker starting. Database Version: {Version}", await databaseService.GetVersion(stoppingToken));

            // Start Kafka consumption in background
            var consumeTask = consumerService.StartConsumingAsync(stoppingToken);

            // Run flush loop
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(FlushIntervalSeconds));

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await FlushBatchAsync(stoppingToken);
            }

            // Final flush on shutdown
            await FlushBatchAsync(stoppingToken);

            try
            {
                await consumeTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
        }

        private async Task FlushBatchAsync(CancellationToken cancellationToken)
        {
            var pings = consumerService.GetBatchedPings();

            if (pings.Count == 0)
                return;

            using var activity = Telemetry.ActivitySource.StartActivity(
                "dbwriter.flush_batch", ActivityKind.Internal);
            activity?.SetTag("batch.size.before_compression", pings.Count);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // TODO: Phase 4.3 - Add compression logic here
                var compressedPings = await compressionService.ApplyTemporalCompression(pings);

                await databaseService.BulkInsertPingsAsync(compressedPings, cancellationToken);

                FleetMetrics.GpsPingsCompressedToDb.Inc(compressedPings.Count);
                activity?.SetTag("batch.size.after_compression", compressedPings.Count);

                await databaseService.UpsertLatestStateAsync(compressedPings, cancellationToken);
                consumerService.ClearBatch();
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
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
