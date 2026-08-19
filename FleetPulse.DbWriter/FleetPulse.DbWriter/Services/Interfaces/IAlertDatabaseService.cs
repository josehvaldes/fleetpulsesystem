using FleetPulse.DbWriter.Models.DB;


namespace FleetPulse.DbWriter.Services.Interfaces
{
    public interface IAlertDatabaseService
    {

        public Task<string?> AddAlert(AlertDb alert);

        public Task<IEnumerable<AlertDb>> GetAlertsByDriverId(string driverId);

        public Task<IEnumerable<AlertDb>> GetAlertsByDateRange(DateTime startDate, DateTime endDate);

    }
}
