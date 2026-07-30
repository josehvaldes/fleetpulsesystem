namespace FleetPulse.DbWriter
{
    /// <summary>
    /// Sample of a background service that logs a message every second. This is a placeholder for the actual worker logic.
    /// </summary>
    /// <param name="logger"></param>
    public class Worker(ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
