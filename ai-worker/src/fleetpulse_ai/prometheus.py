from threading import Lock

from prometheus_client import Counter, Gauge, Histogram, start_http_server

# Define custom metrics

PINGS_RECEIVED = Counter(
    'fleetpulse_ai_pings_received_total',
    'Total pings received from Kafka',
)

PINGS_PROCESSED = Counter(
    'fleetpulse_ai_pings_processed_total',
    'Total pings evaluated by AI worker',
    ['anomaly_detected']  # 'true' or 'false'
)

ANOMALY_DETECTION_DURATION = Histogram(
    'fleetpulse_ai_anomaly_detection_seconds',
    'Time spent in LLM anomaly detection',
    ['anomaly_type'],  # e.g., 'working_zone_violation'
    buckets=[.01, .05, .1, .25, .5, 1, 2, 5]
)

ALERTS_PUBLISHED = Counter(
    'fleetpulse_ai_alerts_published_total',
    'Total alerts published to ai-alerts topic',
    ['severity']  # 'low', 'medium', 'high'
)

KAFKA_LAG = Gauge(
    'fleetpulse_ai_kafka_lag_messages',
    'Estimated consumer lag for AI worker'
)

_PROMETHEUS_STARTED = False
_PROMETHEUS_LOCK = Lock()


def setup_prometheus(port: int = 8000) -> None:
    """Starts the Prometheus HTTP endpoint once per process."""
    global _PROMETHEUS_STARTED

    with _PROMETHEUS_LOCK:
        if _PROMETHEUS_STARTED:
            return

        start_http_server(port)
        _PROMETHEUS_STARTED = True
 


