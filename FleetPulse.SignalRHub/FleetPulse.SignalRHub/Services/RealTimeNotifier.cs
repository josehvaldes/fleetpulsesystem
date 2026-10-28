using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Contracts.Response.Alerts;
using FleetPulse.Contracts.Response.GpsPing;
using FleetPulse.Domain.Entities;
using FleetPulse.SignalRHub.Configuration;
using FleetPulse.SignalRHub.Hubs;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

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
            var gpsPingResponse = payload.Adapt<GpsPingResponse>();
            return _hubContext.Clients.All.SendAsync(GpsPingCallbackMethod, gpsPingResponse, cancellationToken);
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
