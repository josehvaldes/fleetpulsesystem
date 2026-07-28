from fleetpulse_ai.events.violation_event import ViolationEvent
from confluent_kafka import Producer
import sys
from fleetpulse_ai.models.alert_event import AlertEvent
from fleetpulse_ai.settings import settings


def delivery_report(err, msg):
    """Delivery report callback called (from a separate thread) on successful or failed delivery of the message."""
    if err is not None:
        print(f"Message delivery failed: {err}", file=sys.stderr)
    else:
        print(f"Message delivered to {msg.topic()} [{msg.partition()}] at offset {msg.offset()}")

class AlertManager:

    def __init__(self):
        self.producer = None

    async def __aenter__(self):
        try:
            config = {'bootstrap.servers': settings.kafka_bootstrap_servers}
            self.producer = Producer(config)
            return self
        except Exception as e:
            print(f"Failed to initialize Kafka producer: {e}", file=sys.stderr)
            sys.exit(1)

    async def __aexit__(self, exc_type, exc_val, exc_tb):
        self.producer.flush()


    async def handle_alert(self, alert: AlertEvent):
        """Handle a violation event."""
        self.producer.produce(
            topic=settings.kafka_alert_topic,
            key=str(alert.driver_id),
            value=alert.to_json(),
            callback=delivery_report
        )
        self.producer.poll(0)