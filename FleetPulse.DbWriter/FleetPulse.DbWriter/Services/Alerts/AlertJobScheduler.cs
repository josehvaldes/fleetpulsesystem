using FleetPulse.DbWriter.Jobs;
using FleetPulse.DbWriter.Services.Interfaces;
using Hangfire;

namespace FleetPulse.DbWriter.Services.Alerts
{
    public class AlertJobScheduler(IBackgroundJobClient backgroundJobClient) : IAlertJobScheduler
    {
        public void EnqueueStandardAlertProcessing(Guid alertId)
        {
            backgroundJobClient.Enqueue<StandardAlertJob>(x => x.ProcessAlertAsync(alertId, CancellationToken.None));
        }

        public void ScheduleEscalation(Guid alertId, TimeSpan delay)
        {
            backgroundJobClient.Schedule<EscalationJob>(x => x.CheckAndEscalateAsync(alertId, CancellationToken.None), delay);
        }
    }
}
