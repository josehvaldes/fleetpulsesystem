using FleetPulse.Drivers.Contracts.Response;
using FleetPulse.Drivers.Domain.Entities;
using Mapster;

namespace FleetPulse.Drivers.Api
{
    internal static class DriversMapping
    {
        public static void Register()
        {
            TypeAdapterConfig<LatestDriverState, LastestDriverStateResponse>
                .NewConfig()
                .Map(dest => dest.LastSeen, src => src.last_seen.ToString("o"))
                .Map(dest => dest.DriverId, src => src.driver_id)
                .Map(dest => dest.Status, src => src.status)
                .Map(dest => dest.Longitude, src => src.longitude)
                .Map(dest => dest.Latitude, src => src.latitude)
                .Map(dest => dest.Speed, src => src.speed)
                .Map(dest => dest.Heading, src => src.heading);

            TypeAdapterConfig<GpsPing, GpsPingResponse>
                .NewConfig()
                .Map(dest => dest.Timestamp, src => src.timestamp.ToString("o"))
                .Map(dest => dest.DriverId, src => src.driver_id)
                .Map(dest => dest.Latitude, src => src.latitude)
                .Map(dest => dest.Longitude, src => src.longitude)
                .Map(dest => dest.Speed, src => src.speed)
                .Map(dest => dest.Heading, src => src.heading);
        }
    }
}
