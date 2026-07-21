using FleetPulse.SignalRHub.Contracts.Response;

namespace FleetPulse.SignalRHub.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string username, string password, CancellationToken none);
    }
}
