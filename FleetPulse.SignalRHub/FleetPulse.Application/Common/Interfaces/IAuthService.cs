using FleetPulse.Application.Common.Dtos;

namespace FleetPulse.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<AuthenticationResult> LoginAsync(string username, string password, CancellationToken cancellationToken);
    }
}
