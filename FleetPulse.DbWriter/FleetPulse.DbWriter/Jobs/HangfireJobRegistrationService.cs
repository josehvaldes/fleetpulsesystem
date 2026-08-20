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
            _recurringJobManager.AddOrUpdate<AlertProcessor>(
                "AlertWorker",
                worker => worker.ExecuteAsync(CancellationToken.None),
                Cron.MinuteInterval(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
