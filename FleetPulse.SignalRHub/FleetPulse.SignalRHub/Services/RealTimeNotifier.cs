using FleetPulse.Application.Common.Interfaces;
using FleetPulse.SignalRHub.Hubs;
using FleetPulse.SignalRHub.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using FleetPulse.Application.Common.Dtos;
using Mapster;
using FleetPulse.Contracts.Response;
using FleetPulse.Domain.Entities;

namespace FleetPulse.SignalRHub.Services
{
    public class RealTimeNotifier : IRealTimeNotifier
    {
        private readonly IHubContext<FleetHub> _hubContext;
        private readonly SignalRSettings _settings;

        public RealTimeNotifier(IHubContext<FleetHub> hubContext, IOptions<SignalRSettings> signalRSettings)
        {
            _hubContext = hubContext;
            _settings = signalRSettings.Value;
        }

        public string AlertCallbackMethod => _settings.AlertCallbackMethod;

        public string GpsPingCallbackMethod => _settings.GpsPingCallbackMethod;

        public Task SendAlertToAllAsync(Alert payload, CancellationToken cancellationToken = default)
        {
            var alertResponse = payload.Adapt<AlertResponse>();
            return _hubContext.Clients.All.SendAsync(AlertCallbackMethod, alertResponse, cancellationToken);
        }

        public Task SendgpsPingToAllAsync(GpsPing payload, CancellationToken cancellationToken = default)
        {
            return _hubContext.Clients.All.SendAsync(GpsPingCallbackMethod, payload, cancellationToken);
        }

        public Task SendToAllAsync(string method, object payload, CancellationToken cancellationToken = default)
        {
            return _hubContext.Clients.All.SendAsync(method, payload, cancellationToken);
        }

        public Task SendToGroupAsync(string group, string method, object payload, CancellationToken cancellationToken = default)
        {
            return _hubContext.Clients.Group(group).SendAsync(method, payload, cancellationToken);
        }
    }
}
