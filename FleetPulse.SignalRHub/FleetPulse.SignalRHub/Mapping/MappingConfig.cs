using FleetPulse.Contracts.Response;
using FleetPulse.Domain.Entities;
using Mapster;

namespace FleetPulse.SignalRHub.Mapping
{
    public static class MappingConfig
    {
        public static void RegisterMappings() 
        {
            // Register your Mapster mappings here
            TypeAdapterConfig<LatestDriverState, LastestDriverStateResponse>
                .NewConfig()
                .Map(dest => dest.LastSeen, src => src.Last_Seen.ToString("o"))
                .Map(dest => dest.DriverId, src => src.Driver_Id);

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
                .Map(dest => dest.AutoScale, src => src.autoscale)
                .Map(dest => dest.Status, src => src.status.ToString())
                .Map(dest => dest.RaisedAt, src => src.raised_at);

            TypeAdapterConfig<GpsPing, GpsHistoryResponse>
                .NewConfig()
                .Map(dest => dest.Timestamp, src => src.Timestamp.ToString("o"))
                .Map(dest => dest.DriverId, src => src.DriverId);

        }
    }
}
