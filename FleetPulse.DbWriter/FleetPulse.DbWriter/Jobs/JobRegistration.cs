using Hangfire;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Jobs
{
    public static class JobRegistration
    {
        public static void RegisterJobs()
        {
            RecurringJob.AddOrUpdate<AlertProcessor>(
                "AlertWorker",
                worker => worker.ExecuteAsync(default),
                Cron.MinuteInterval(2));
        }
    }
}
