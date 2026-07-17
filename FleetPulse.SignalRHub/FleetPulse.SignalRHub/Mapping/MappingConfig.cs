using FleetPulse.SignalRHub.Contracts.Response;
using FleetPulse.SignalRHub.Model;
using Mapster;

namespace FleetPulse.SignalRHub.Mapping
{
    public static class MappingConfig
    {
        public static void RegisterMappings() 
        {
            // Register your Mapster mappings here
            TypeAdapterConfig<LatestDriverStateDto, LastestDriverStateResponse>
                .NewConfig()
                .Map(dest => dest.LastSeen, src => src.Last_Seen.ToString("o"))
                .Map(dest => dest.DriverId, src => src.Driver_Id);
            TypeAdapterConfig<GpsPingDto, GpsHistoryResponse>
                .NewConfig()
                .Map(dest => dest.Timestamp, src => src.Timestamp.ToString("o"))
                .Map(dest => dest.DriverId, src => src.Driver_Id);
            TypeAdapterConfig<AlertDto, AlertResponse>
                .NewConfig()
                .Map(dest => dest.CreatedAt, src => src.Created_at.ToString("o"))
                .Map(dest => dest.DriverId, src => src.Driver_id);
        }
    }
}
