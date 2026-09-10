using Confluent.Kafka;
using FleetPulse.DbWriter.Configuration;
using FleetPulse.DbWriter.Infrastructure;
using FleetPulse.DbWriter.MetricsConfig;
using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Models.DB;
using FleetPulse.DbWriter.Services.Interfaces;
using Mapster;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FleetPulse.DbWriter.Services.Alerts
{
    public class AlertMessageHandler(
        ILogger<AlertMessageHandler> logger,
        IAlertDatabaseService alertDatabaseService,
        IAlertJobScheduler alertJobScheduler,
        IOptions<KafkaSettings> kafkaSettings) : IKafkaMessageHandler
    {
        private readonly KafkaSettings _settings = kafkaSettings.Value;
        private static readonly TimeSpan EscalationDelay = TimeSpan.FromSeconds(10);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<bool> HandleAsync(string message, CancellationToken cancellationToken)
        {
            var alert = DeserializeAlert(message);
            if (alert is null)
            {
                FleetMetrics.AlertsProcessingErrors.WithLabels(new[] { ErrorLabel.DeserializationError.ToString(), _settings.AlertTopic }).Inc();
                return false;
            }

            var alertDb = alert.Adapt<AlertDb>();
            var alertId = await alertDatabaseService.AddAlertAsync(alertDb, cancellationToken);
            alertDb.id = alertId;

            if (alertDb.risk_level == RiskLevel.High && alertDb.auto_escalate)
            {
                alertJobScheduler.ScheduleEscalation(alertId, EscalationDelay);
            }

            alertJobScheduler.EnqueueStandardAlertProcessing(alertId);
            return true;
        }

        private AlertDto? DeserializeAlert(string message)
        {
            try
            {
                return JsonSerializer.Deserialize<AlertDto>(message, JsonOptions);
            }
            catch (JsonException)
            {
                logger.LogWarning("Failed to deserialize message from Kafka: {Message}", message);
                return null;
            }
        }
    }
}
