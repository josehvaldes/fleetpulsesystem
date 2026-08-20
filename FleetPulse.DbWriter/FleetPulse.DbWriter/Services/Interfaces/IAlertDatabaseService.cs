using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Models.DB;


namespace FleetPulse.DbWriter.Services.Interfaces
{
    public interface IAlertDatabaseService
    {

        public Task<Guid> AddAlert(AlertDb alert);

        public Task<IEnumerable<AlertDb>> GetAlertsByDriverId(string driverId);

        public Task<IEnumerable<AlertDb>> GetAlertsByDateRange(DateTime startDate, DateTime endDate);

        public Task<IEnumerable<AlertDb>> GetAlertsByStatusDateRange(AlertStatus status, DateTime startDate, DateTime endDate);

    }
}
