using FleetPulse.Domain.Entities;
using FleetPulse.Infrastructure.Kafka.Dtos;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.Infrastructure.Kafka
{
    public static class KafkaMapping
    {

        public static void RegisterMappings()
        {
            TypeAdapterConfig<AlertDto, Alert>
                .NewConfig()
                .Map(dest => dest.raised_at, src => src.created_at)
                .Map(dest => dest.event_latitude, src => src.exit_location.latitude)
                .Map(dest => dest.event_longitude, src => src.exit_location.longitude)
                .Map(dest => dest.risk_level, src => src.agent_risk_level)
                .Map(dest => dest.assessment, src => src.agent_assessment)
                .Map(dest => dest.recommendation, src => src.agent_recommendation)
                ;

            //The GpsPingDto to GpsPing mapping is straightforward and does not require any custom mapping.

        }
    }
}
