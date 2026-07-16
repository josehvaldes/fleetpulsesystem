from abc import ABC, abstractmethod
from datetime import datetime
import paho.mqtt.client as mqtt
import json

from fleetpulse.drivers import Driver

class MQTTPublisherInterface(ABC):
    @abstractmethod
    async def publish_gps(self, driver: Driver):
        pass

    async def publish(self, message: dict):
        pass

class MQTTPublisher(MQTTPublisherInterface):
    def __init__(self, broker: str, port: int = 1883):
        self.broker = broker
        self.port = port
        self.client = mqtt.Client()
        
    
    async def __aenter__(self) -> 'MQTTPublisher':
        # Configure QoS 1 for at-least-once delivery
        self.client.connect_async(self.broker, self.port, keepalive=60)
        self.client.loop_start()
        return self
    
    async def __aexit__(self, exc_type, exc, tb) -> None:
        self.client.loop_stop()
        self.client.disconnect()


    async def publish_gps(self, driver: Driver):
        payload = {
            "driver_id": driver.driver_id,
            "timestamp": datetime.now().isoformat(),
            "latitude": driver.current_position.latitude,
            "longitude": driver.current_position.longitude,
            "speed": driver.speed_factor,
            "heading": 0.0,  # Placeholder for heading
            "accuracy": 5.0
        }
        topic = f"fleet_pulse/{driver.driver_id}/gps"
        self.client.publish(topic, json.dumps(payload), qos=1)
    
    async def publish(self, message: dict):
        topic = f"fleet_pulse/{message['driver_id']}/gps"
        try :
            self.client.publish(topic, json.dumps(message), qos=1)
        except Exception as e:
            print(f"Error publishing message: {e}")


class MQTTMockPublisher(MQTTPublisherInterface):
    def __init__(self):
        self.published_messages = []
    
    async def __aenter__(self) -> 'MQTTMockPublisher':
        return self
    
    async def __aexit__(self, exc_type, exc, tb) -> None:
        pass

    async def publish_gps(self, driver: Driver):
        topic = f"fleet_pulse/{driver.driver_id}/gps"
        payload = {
            "driver_id": driver.driver_id,
            "timestamp": datetime.now().isoformat(),
            "latitude": driver.current_position.latitude,
            "longitude": driver.current_position.longitude,
            "speed": driver.speed_factor,
            "heading": 0.0,  # Placeholder for heading
            "accuracy": 5.0,
            #"topic": topic
        }
        self.published_messages.append(payload)

    async def publish(self, message: dict):
        topic = f"fleet_pulse/{message['driver_id']}/gps"
        #message["topic"] = topic
        self.published_messages.append(message)
