from confluent_kafka import Producer
import sys
from fleetpulse_ai.models.alert_event import AlertEvent
from fleetpulse_ai.settings import settings
from fleetpulse_ai.logging_config import get_logger
logger = get_logger(__name__)

def delivery_report(err, msg):
    """Delivery report callback called (from a separate thread) on successful or failed delivery of the message."""
    if err is not None:
        logger.error("kafka_delivery_failed", error=str(err))
    else:
        logger.info("kafka_delivery_success", topic=msg.topic(), partition=msg.partition(), offset=msg.offset())

class AlertManager:

    def __init__(self):
        self.producer = None

    async def __aenter__(self):
        try:
            config = {'bootstrap.servers': settings.kafka_bootstrap_servers}
            self.producer = Producer(config)
            return self
        except Exception as e:
            logger.error("kafka_producer_init_failed", error=str(e))
            sys.exit(1)

    async def __aexit__(self, exc_type, exc_val, exc_tb):
        self.producer.flush()


    async def handle_alert(self, alert: AlertEvent, metadata: dict) -> None:
        """Handle a violation event."""
        headers = []
        if alert.traceparent:
            headers.append(("traceparent", metadata.get("traceparent", "").encode("utf-8")))
        if alert.tracestate:
            headers.append(("tracestate", metadata.get("tracestate", "").encode("utf-8")))

        self.producer.produce(
            topic=settings.kafka_alert_topic,
            key=str(alert.driver_id),
            value=alert.to_json(),
            headers=headers,
            callback=delivery_report
        )
        self.producer.poll(0)