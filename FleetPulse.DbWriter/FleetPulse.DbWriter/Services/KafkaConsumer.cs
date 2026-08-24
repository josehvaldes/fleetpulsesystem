using Confluent.Kafka;
using FleetPulse.DbWriter.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Services
{
    public abstract class KafkaConsumer
    {
        protected void LogKafkaMessage(KafkaLogThrottle throttle, LogMessage msg)
        {
            // Drop the noisiest, lowest-value levels entirely during outages
            if (msg.Level is SyslogLevel.Info or SyslogLevel.Debug) return;
            var level = msg.Level switch
            {
                SyslogLevel.Emergency or SyslogLevel.Alert or SyslogLevel.Critical => LogLevel.Critical,
                SyslogLevel.Error => LogLevel.Error,
                SyslogLevel.Warning or SyslogLevel.Notice => LogLevel.Warning,
                _ => LogLevel.Trace
            };
            throttle.Emit(level, $"[{msg.Facility}] {msg.Message}");
        }

        protected static ConsumerConfig CreateConsumerConfig(KafkaSettings settings)
        {
            return new ConsumerConfig
            {
                BootstrapServers = settings.BootstrapServers,
                GroupId = settings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest, // Start from the earliest message if no offset is found
                EnableAutoCommit = false,  // Manual commit for reliability
                SessionTimeoutMs = 10000,
                MaxPollIntervalMs = 300000,

                ReconnectBackoffMs = 5000,
                ReconnectBackoffMaxMs = 60000,
                SocketConnectionSetupTimeoutMs = 10000,

                // Silence chatty librdkafka logs
                LogConnectionClose = false,
                LogQueue = false
            };
        }
    }
}
