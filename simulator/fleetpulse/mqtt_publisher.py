from abc import ABC, abstractmethod
from datetime import datetime
import paho.mqtt.client as mqtt
from paho.mqtt.properties import Properties
from paho.mqtt.packettypes import PacketTypes

import json
from utils.logging_config import get_logger    
logger = get_logger(__name__)

from fleetpulse.drivers import Driver

class MQTTPublisherInterface(ABC):

    @abstractmethod
    async def publish(self, message: dict, metadata: dict = None):
        pass



class MQTTPublisher(MQTTPublisherInterface):
    def __init__(self, broker: str, port: int = 1883):
        self.broker = broker
        self.port = port
        # IMPORTANT: protocol=MQTTv5 is mandatory for User Properties
        try:
            self.client = mqtt.Client(
                callback_api_version=mqtt.CallbackAPIVersion.VERSION2,
                protocol=mqtt.MQTTv5
            )
            self.client.on_connect = self._on_connect

        except Exception as e:
            logger.error("Error initializing MQTT client. paho-mqtt 1.x fallback: %s", e)
            # paho-mqtt 1.x fallback
            self.client = mqtt.Client(protocol=mqtt.MQTTv5)

    
    def _on_connect(self, client, userdata, flags, reason_code, properties):
        if reason_code == 0:
            logger.info("Connected to MQTT broker")
        else:
            logger.error(
                "MQTT connection failed: reason_code=%s",
                reason_code
            )

    async def __aenter__(self) -> 'MQTTPublisher':
        # Configure QoS 1 for at-least-once delivery
        self.client.connect(self.broker, self.port, keepalive=60)
        self.client.loop_start()
        return self
    
    async def __aexit__(self, exc_type, exc, tb) -> None:
        self.client.loop_stop()
        self.client.disconnect()

    @staticmethod
    def _build_properties(metadata: dict | None) -> Properties | None:
        """
        Convert tracing metadata into MQTT v5 PUBLISH User Properties.
        Only non-empty values are attached.
        """
        if not metadata:
            return None

        user_props: list[tuple[str, str]] = []
        for key in ("traceparent", "tracestate"):
            value = metadata.get(key)
            if value:
                # MQTT user property values MUST be strings
                user_props.append((key, str(value)))

        if not user_props:
            return None

        props = Properties(PacketTypes.PUBLISH)
        props.UserProperty = user_props  # list of (k, v) tuples
        return props
    
    async def publish(self, message: dict, metadata: dict = None):
        topic = f"fleet_pulse/{message['driver_id']}/gps"
        try:
            properties = self._build_properties(metadata)
            info = self.client.publish(
                topic=topic, 
                payload= json.dumps(message), 
                qos=1, 
                properties=properties)

            # info.rc == mqtt.MQTT_ERR_SUCCESS means accepted by the client
            if info.rc != mqtt.MQTT_ERR_SUCCESS:
                logger.warning(f"PUBLISH rc={info.rc} on topic {topic}")

        except Exception as e:
            logger.error(f"Error publishing message: {e}", exc_info=True)




######################################################################
# Mock publisher for testing purposes
######################################################################
class MQTTMockPublisher(MQTTPublisherInterface):
    def __init__(self):
        self.published_messages = []
    
    async def __aenter__(self) -> 'MQTTMockPublisher':
        return self
    
    async def __aexit__(self, exc_type, exc, tb) -> None:
        pass

    async def publish(self, message: dict, metadata: dict = None):
        self.published_messages.append(message)
