using FleetPulse.Alerts.Contracts.Response;
using FleetPulse.Alerts.Domain.Entities;
using Mapster;

namespace FleetPulse.Alerts.Api
{
    internal static class AlertsMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<Alert, AlertResponse>.NewConfig()
                .Map(dest => dest.Id, src => src.id)
                .Map(dest => dest.DriverId, src => src.driver_id)
                .Map(dest => dest.EventLatitude, src => src.event_latitude)
                .Map(dest => dest.EventLongitude, src => src.event_longitude)
                .Map(dest => dest.ExitSpeed, src => src.exit_speed)
                .Map(dest => dest.ExitTime, src => src.exit_time)
                .Map(dest => dest.ZoneName, src => src.zone_name)
                .Map(dest => dest.ZoneType, src => src.zone_type)
                .Map(dest => dest.RiskLevel, src => src.risk_level.ToString())
                .Map(dest => dest.Assessment, src => src.assessment)
                .Map(dest => dest.Recommendation, src => src.recommendation)
                .Map(dest => dest.AutoEscalate, src => src.auto_escalate)
                .Map(dest => dest.Status, src => src.status.ToString())
                .Map(dest => dest.RaisedAt, src => src.raised_at);
        }
    }
}
