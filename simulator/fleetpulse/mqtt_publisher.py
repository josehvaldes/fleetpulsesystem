from datetime import datetime
import paho.mqtt.client as mqtt
import json

from fleetpulse.drivers import Driver

class TelemetryPublisher:
    def __init__(self, broker: str, port: int = 1883):
        self.client = mqtt.Client()
        # Configure QoS 1 for at-least-once delivery
        self.client.connect_async(broker, port, keepalive=60)
    
    async def __aenter__(self) -> 'TelemetryPublisher':
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