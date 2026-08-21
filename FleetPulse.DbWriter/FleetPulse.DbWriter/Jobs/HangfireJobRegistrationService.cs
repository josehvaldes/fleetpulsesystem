using Hangfire;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Jobs
{
    public class HangfireJobRegistrationService(IRecurringJobManager _recurringJobManager) : IHostedService
    {

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _recurringJobManager.AddOrUpdate<CleanupAlertProcessor>(
                "CleanupAlertWorker",
                worker => worker.ExecuteAsync(CancellationToken.None),
                Cron.Hourly );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
