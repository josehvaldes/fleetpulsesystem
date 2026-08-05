from abc import ABC, abstractmethod
import json
import signal

from typing import Callable, Awaitable
from confluent_kafka import Consumer, KafkaError, KafkaException
from fleetpulse_ai.logging_config import get_logger
from fleetpulse_ai.prometheus import PINGS_RECEIVED

logger = get_logger(__name__)

class IKafkaConsumer (ABC):

    @abstractmethod
    async def consume(self, 
        message_handler: Callable[[dict], Awaitable[None]],  # Should be async
        poll_timeout: float = 1.0,
        deserialize_json: bool = True,):
        pass


class KafkaConsumer(IKafkaConsumer):
    """
    A wrapper around confluent_kafka.Consumer configured for Redpanda.
    Handles graceful shutdown, auto-offset management, and JSON deserialization.
    """

    def __init__(
        self,
        bootstrap_servers: str,
        group_id: str,
        topics: list[str],
        auto_offset_reset: str = "earliest",
        enable_auto_commit: bool = True,
    ):
        """
        Args:
            bootstrap_servers: Comma-separated list of Redpanda brokers.
                            Example: "localhost:9092" or "redpanda-0:9092,redpanda-1:9092"
            group_id: Consumer group ID for offset tracking and load balancing.
            topics: List of topic names to subscribe to.
            auto_offset_reset: Where to start if no committed offset exists ("earliest" or "latest").
            enable_auto_commit: Whether to commit offsets automatically in the background.
        """
        self.topics = topics
        self._running = False

        self.config = {
            "bootstrap.servers": bootstrap_servers,
            "group.id": group_id,
            "auto.offset.reset": auto_offset_reset,
            "enable.auto.commit": enable_auto_commit,
            # Redpanda-specific: these settings improve latency for high-throughput streams
            "fetch.wait.max.ms": 10,
            "session.timeout.ms": 6000,
        }

        self.consumer = Consumer(self.config)
        self.consumer.subscribe(self.topics)

        # Graceful shutdown on SIGINT / SIGTERM
        signal.signal(signal.SIGINT, self._signal_handler)
        signal.signal(signal.SIGTERM, self._signal_handler)

        logger.info(
            "kafka_consumer_initialized",
            brokers=bootstrap_servers,
            group_id=group_id,
            topics=topics,
        )

    def _signal_handler(self, signum, frame):
        """Handle shutdown signals gracefully."""

        logger.info("shutdown_signal_received", signal_number=signum)
        self._running = False

    async def consume(
        self,
        message_handler: Callable[[dict], Awaitable[None]],  # Should be async
        poll_timeout: float = 5.0, 
        deserialize_json: bool = True,
    ) -> None:
        """
        Start consuming messages in a blocking loop.

        Args:
            message_handler: Callback function that receives each message as a dict.
            poll_timeout: Max seconds to wait for a message on each poll iteration.
            deserialize_json: If True, attempts to parse message value as JSON.
        """
        self._running = True
        logger.info("consumer_loop_started", topics=self.topics)

        try:
            while self._running:
                msg = self.consumer.poll(timeout=poll_timeout)

                if msg is None:
                    continue

                if msg.error():
                    logger.error("kafka_message_error", error=str(msg.error()))
                    self._handle_error(msg)
                    continue

                PINGS_RECEIVED.inc()
                # Extract metadata
                payload = {
                    "topic": msg.topic(),
                    "partition": msg.partition(),
                    "offset": msg.offset(),
                    "key": msg.key().decode("utf-8") if msg.key() else None,
                    "value": msg.value(),
                    "timestamp": msg.timestamp()[1] if msg.timestamp()[0] != 0 else None,
                }

                # Deserialize payload value
                if deserialize_json:
                    try:
                        value_wrapper = json.loads(msg.value().decode("utf-8"))
                        value = value_wrapper.get("payload") if isinstance(value_wrapper, dict) else value_wrapper
                        payload["value"] = json.loads(value) if isinstance(value, str) else value
                    except (json.JSONDecodeError, UnicodeDecodeError) as e:
                        logger.warning("json_decode_failed", error=str(e))
                        payload["value"] = msg.value().decode("utf-8", errors="replace")

                # Hand off to the AI worker
                try:
                    await message_handler(payload["value"]) # Pass only the deserialized value to the handler
                except Exception as e:
                    logger.exception("message_handler_failed", error=str(e))

        except KafkaException as e:
            logger.error("kafka_exception", error=str(e))
        finally:
            self.close()

    def _handle_error(self, msg) -> None:
        """Handle message-level errors from Redpanda/Kafka."""
        error = msg.error()
        if error.code() == KafkaError._PARTITION_EOF:
            logger.debug(
                "kafka_partition_eof",
                topic=msg.topic(),
                partition=msg.partition(),
                offset=msg.offset(),
            )
        elif error.code() == KafkaError._ALL_BROKERS_DOWN:
            logger.critical("kafka_brokers_down")
            self._running = False
        else:
            logger.error("kafka_consumer_error", error=str(error))

    def close(self) -> None:
        """Cleanly close the consumer and release resources."""
        logger.info("kafka_consumer_closing")
        self.consumer.close()
        logger.info("kafka_consumer_closed")

