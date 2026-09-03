using System.Threading;
using System.Threading.Tasks;

namespace FleetPulse.Application.Common.Interfaces
{
    /// <summary>
    /// Application-facing port for broadcasting real-time messages. Implemented by the SignalR layer.
    /// </summary>
    public interface IRealTimeNotifier
    {
        string AlertCallbackMethod { get; }
        string GpsPingCallbackMethod { get; }

        Task SendToAllAsync(string method, object payload, CancellationToken cancellationToken = default);
        Task SendToGroupAsync(string group, string method, object payload, CancellationToken cancellationToken = default);
    }
}
