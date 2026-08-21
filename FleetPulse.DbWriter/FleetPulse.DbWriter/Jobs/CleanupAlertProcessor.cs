using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Services.Interfaces;
using Hangfire;

namespace FleetPulse.DbWriter.Jobs
{
    public class CleanupAlertProcessor(IAlertDatabaseService databaseService,
        ILogger<CleanupAlertProcessor> logger)
    {
        //for now, disable concurrent execution of this job to avoid processing the same alerts multiple times 
        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        public async Task ExecuteAsync(CancellationToken cancellationToken) 
        {
            logger.LogInformation("AlertProcessor job is starting.");
            var toDate = DateTime.UtcNow;
            var fromDate = toDate.AddDays(-1);
            var alerts = await databaseService.GetAlertsByStatusDateRangeAsync( AlertStatus.Closed, fromDate, toDate, cancellationToken);
            logger.LogInformation($"Retrieved {alerts.Count()} closed alerts from the database.");
        }

    }
}
