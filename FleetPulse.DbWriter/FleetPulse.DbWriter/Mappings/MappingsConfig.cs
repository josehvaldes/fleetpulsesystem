using FleetPulse.DbWriter.Models;
using FleetPulse.DbWriter.Models.DB;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Mappings
{
    public class MappingsConfig
    {
        public static void ConfigureMappings()
        {
            TypeAdapterConfig<AlertDto, AlertDb>.NewConfig()
                .Map(dest => dest.id, src => src.Id)
                .Map(dest => dest.driver_id, src => src.DriverId)
                .Map(dest => dest.event_latitude, src => src.ExitLocation.Latitude)
                .Map(dest => dest.event_longitude, src => src.ExitLocation.Longitude)
                .Map(dest => dest.exit_speed, src => src.ExitSpeed)
                .Map(dest => dest.exit_time, src => src.ExitTime)
                .Map(dest => dest.zone_name, src => src.ZoneName)
                .Map(dest => dest.zone_type, src => src.ZoneType)
                .Map(dest => dest.risk_level, src => Enum.Parse<RiskLevel>(src.AgentRiskLevel, ignoreCase: true))
                .Map(dest => dest.assessment, src => src.AgentAssessment)
                .Map(dest => dest.recommendation, src => src.AgentRecommendation)
                .Map(dest => dest.autoscale, src => src.AgentAutoEscalate)
                .Map(dest => dest.raised_at, src => src.CreatedAt);
        }
    }
}
