using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Jobs
{
    public class EscalationJob(IAlertDatabaseService _databaseService,
        ILogger<EscalationJob> _logger)
    {
        public async Task CheckAndEscalateAsync(Guid alertId, CancellationToken ct) 
        {
            var alert = await _databaseService.GetAlertByIdAsync(alertId, ct);

            if (alert == null) 
            {
                _logger.LogWarning("Alert with ID: {AlertId} not found.", alertId);
                return; 
            }
            // Check if the alert is still open and has not been escalated
            if (alert.status == AlertStatus.New && alert.autoscale)
            {
                // Escalate the alert
                _logger.LogInformation("Escalating alert with ID: {AlertId}", alertId);
                // Add escalation logic here
            }
        }
    }
}
