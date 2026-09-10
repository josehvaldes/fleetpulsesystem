namespace FleetPulse.DbWriter.Services.Interfaces
{
    public interface IAlertJobScheduler
    {
        void EnqueueStandardAlertProcessing(Guid alertId);
        void ScheduleEscalation(Guid alertId, TimeSpan delay);
    }
}
