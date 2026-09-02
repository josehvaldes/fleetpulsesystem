using FleetPulse.Contracts.Response;

namespace FleetPulse.SignalRHub.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string username, string password, CancellationToken none);
    }
}
