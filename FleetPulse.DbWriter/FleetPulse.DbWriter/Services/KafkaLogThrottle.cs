using System;
using System.Collections.Generic;
using System.Text;

namespace FleetPulse.DbWriter.Services
{
    public sealed class KafkaLogThrottle
    {
        private readonly ILogger _logger;
        private readonly string _consumerName;
        private readonly TimeSpan _suppressWindow;

        private int _suppressedSinceLastEmit;
        private DateTime _lastEmit = DateTime.MinValue;
        private readonly object _gate = new();

        public KafkaLogThrottle(ILogger logger, string consumerName, TimeSpan? suppressWindow = null)
        {
            _logger = logger;
            _consumerName = consumerName;
            _suppressWindow = suppressWindow ?? TimeSpan.FromSeconds(30);
        }

        public void Emit(LogLevel level, string message)
        {
            lock (_gate)
            {
                var now = DateTime.UtcNow;
                if (now - _lastEmit >= _suppressWindow)
                {
                    if (_suppressedSinceLastEmit > 0)
                        _logger.Log(level,
                            "rdkafka {Consumer} (suppressed {Count} similar messages): {Message}",
                            _consumerName, _suppressedSinceLastEmit, message);
                    else
                        _logger.Log(level, "rdkafka {Consumer}: {Message}",
                            _consumerName, message);

                    _lastEmit = now;
                    _suppressedSinceLastEmit = 0;
                }
                else
                {
                    _suppressedSinceLastEmit++;
                }
            }
        }
    }
}
