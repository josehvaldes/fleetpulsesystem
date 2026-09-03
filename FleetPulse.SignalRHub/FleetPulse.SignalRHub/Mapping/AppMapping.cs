using FleetPulse.Contracts.Response;
using FleetPulse.Domain.Entities;
using FleetPulse.Infrastructure.Kafka;
using Mapster;

namespace FleetPulse.SignalRHub.Mapping
{
    public static class AppMapping
    {
        public static void RegisterMappings() 
        {
            // Register your Mapster mappings here
            TypeAdapterConfig<LatestDriverState, LastestDriverStateResponse>
                .NewConfig()
                .Map(dest => dest.LastSeen, src => src.last_seen.ToString("o"))
                .Map(dest => dest.DriverId, src => src.driver_id);

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
                .Map(dest => dest.AutoScale, src => src.auto_escalate)
                .Map(dest => dest.Status, src => src.status.ToString())
                .Map(dest => dest.RaisedAt, src => src.raised_at);

            TypeAdapterConfig<GpsPing, GpsPingResponse>
                .NewConfig()
                .Map(dest => dest.Timestamp, src => src.timestamp.ToString("o"))
                .Map(dest => dest.DriverId, src => src.driver_id);

            KafkaMapping.RegisterMappings(); // Register Kafka mappings
        }
    }
}
