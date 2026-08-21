using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Services.Interfaces;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Jobs
{
    [Queue("standard-alerts")]
    public class StandardAlertJob(IAlertDatabaseService _databaseService,
        ILogger<StandardAlertJob> _logger)
    {
        public async Task ProcessAlertAsync(Guid alertId, CancellationToken ct)
        {
            var alert = await _databaseService.GetAlertByIdAsync(alertId, ct);
            if (alert == null)
            {
                _logger.LogWarning("Alert with ID: {AlertId} not found.", alertId);
                return;
            }
            // Check if the alert is still open and has not been escalated
            if (alert.status == AlertStatus.New && !alert.autoscale)
            {
                // Process the standard alert
                _logger.LogInformation("Processing standard alert with ID: {AlertId}", alertId);
                // Add standard alert processing logic here
            }
        }
    }
}
