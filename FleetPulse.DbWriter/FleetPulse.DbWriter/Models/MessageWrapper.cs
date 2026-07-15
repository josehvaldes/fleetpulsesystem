using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FleetPulse.DbWriter.Models
{
    public class MessageWrapper
    {
        [JsonPropertyName("payload")]
        public string Payload { get; init; } = default!;

        [JsonPropertyName("kafka_key")]
        public string KafkaKey { get; init; } = default!;

        [JsonPropertyName("client_id")]
        public string ClientId { get; init; } = default!;

    }
}
