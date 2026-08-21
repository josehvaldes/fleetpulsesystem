using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Models.DB;


namespace FleetPulse.DbWriter.Services.Interfaces
{
    public interface IAlertDatabaseService
    {

        public Task<Guid> AddAlertAsync(AlertDb alert, CancellationToken ct);

        public Task<AlertDb?> GetAlertByIdAsync (Guid alertId, CancellationToken ct);

        public Task<IEnumerable<AlertDb>> GetAlertsByDriverIdAsync(string driverId, CancellationToken ct);

        public Task<IEnumerable<AlertDb>> GetAlertsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct);

        public Task<IEnumerable<AlertDb>> GetAlertsByStatusDateRangeAsync(AlertStatus status, DateTime startDate, DateTime endDate, CancellationToken ct);

    }
}
