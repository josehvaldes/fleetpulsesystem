using System.ComponentModel.DataAnnotations;

namespace FleetPulse.SignalRHub.Contracts.Requests
{
    public class LoginRequest
    {
        public Guid UserId { get; set; }
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
