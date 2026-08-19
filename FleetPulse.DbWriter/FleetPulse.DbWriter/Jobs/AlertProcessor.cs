using FleetPulse.DbWriter.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Jobs
{
    public class AlertProcessor(IAlertDatabaseService databaseService)
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken) 
        {
            
            var toDate = DateTime.Now;
            var fromDate = toDate.AddDays(-1);
            var alerts = await databaseService.GetAlertsByDateRange(fromDate, toDate);
            // Process the alerts as needed
        }

    }
}
