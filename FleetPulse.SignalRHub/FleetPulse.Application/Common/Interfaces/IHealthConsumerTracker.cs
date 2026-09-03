namespace FleetPulse.Application.Common.Interfaces
{
    public interface IHealthConsumerTracker
    {
        void RecordHeartbeat();
        DateTime GetLastHeartbeat();
    }
}
