using Confluent.Kafka;
using FleetPulse.Application.Common.Interfaces;
using FleetPulse.Infrastructure.Kafka.HealthChecks;
using FleetPulse.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FleetPulse.Infrastructure.Kafka
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddKafkaDependencies(this IServiceCollection services, ConfigurationManager config) 
        {
            services.Configure<KafkaSettings>(config.GetSection(KafkaSettings.SectionName));

            services.AddKeyedSingleton<IConsumer<string, string>>("gps-pings", (sp, _) =>
            {
                var kafkaConfig = GetKafkaConfig(sp);
                IHealthConsumerTracker tracker = sp.GetRequiredService<IHealthConsumerTracker>();
                ILogger logger = sp.GetRequiredService<ILogger<GpsPingConsumer>>();
                var throttle = new KafkaLogThrottle(logger, "gps-pings");

                return new ConsumerBuilder<string, string>(kafkaConfig)
                    .SetStatisticsHandler((_, _) => tracker.RecordHeartbeat())
                    .SetLogHandler((_, msg) => LogKafkaMessage(throttle, msg))
                    .SetErrorHandler((_, error) =>
                        throttle.Emit(LogLevel.Critical, $"Ping [{error.Code}] {error.Reason}"))
                    .Build();
            });

            services.AddKeyedSingleton<IConsumer<string, string>>("alerts", (sp, _) =>
            {
                var kafkaConfig = GetKafkaConfig(sp);
                ILogger logger = sp.GetRequiredService<ILogger<AlertConsumer>>();
                var throttle = new KafkaLogThrottle(logger, "alerts");

                return new ConsumerBuilder<string, string>(kafkaConfig)
                    .SetLogHandler((_, msg) => LogKafkaMessage(throttle, msg))
                    .SetErrorHandler((_, error) => throttle.Emit(LogLevel.Critical, $"Alerts [{error.Code}] {error.Reason}"))
                    .Build();
            });
            
            services.AddHealthChecks().AddCheck<KafkaConsumerHealthCheck>("kafka_consumer_check");
            services.AddSingleton<IHealthConsumerTracker, KafkaConsumerTracker>();
            
            services.AddBackgroundWorkers(config);            

            return services;
        }


        public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services, ConfigurationManager config)
        {
            // AddHostedService guarantees single instance, start/stop with the host
            services.AddHostedService<GpsPingConsumer>();
            services.AddHostedService<AlertConsumer>();

            return services;
        }

        private static ConsumerConfig GetKafkaConfig(IServiceProvider sp)
        {
            var kafkaConfig = sp.GetRequiredService<IConfiguration>()
                           .GetSection(KafkaSettings.SectionName)
                           .Get<ConsumerConfig>()!;

            kafkaConfig.ReconnectBackoffMs = 5000;
            kafkaConfig.ReconnectBackoffMaxMs = 60000;
            kafkaConfig.SocketConnectionSetupTimeoutMs = 10000;

            // Silence chatty librdkafka logs
            kafkaConfig.LogConnectionClose = false;
            //kafkaConfig.Debug = "none";
            kafkaConfig.LogQueue = false;

            return kafkaConfig;
        }


        // Maps librdkafka syslog-style levels (0=emerg..7=debug) to ILogger levels.
        private static void LogKafkaMessage(KafkaLogThrottle throttle, LogMessage msg)
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

    }
}
