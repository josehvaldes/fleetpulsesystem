using FleetPulse.DbWriter.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Workers
{
    public class AlertWorker(ILogger<AlertWorker> logger, 
        IAlertConsumer alertConsumer) : BackgroundService
    {
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("AlertWorker is starting.");
            var task = alertConsumer.StartConsumingAsync(stoppingToken);
            try 
            {
                logger.LogInformation("AlertWorker is running.");
                await task;
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }            
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("AlertWorker is shutting down");
            await base.StopAsync(cancellationToken);
        }
    }
}
