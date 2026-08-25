using FleetPulse.MockFleetHub.Contracts;
using FleetPulse.MockFleetHub.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FleetPulse.MockFleetHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MockFleetController(IHubContext<FleetHub> _hubContext, ILogger<MockFleetController> _logger) : ControllerBase
    {
        [HttpPost("gpsping")]
        public async Task<IActionResult> PostGpsPing(GpsPingRequest request, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Received GPS ping from driver {DriverId} at {Timestamp}", request.DriverId, request.Timestamp);
            await _hubContext.Clients.All
                                .SendAsync("ReceiveGpsPing", request, stoppingToken);
            return Ok();
        }

        [HttpPost("alert")]
        public async Task<IActionResult> PostAlert(AlertRequest request, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Received alert {AlertId} for driver {DriverId} at {RaisedAt}", request.Id, request.DriverId, request.RaisedAt);
            await _hubContext.Clients.All
                                .SendAsync("ReceiveAlert", request, stoppingToken);
            return Ok();
        }
    }
}
