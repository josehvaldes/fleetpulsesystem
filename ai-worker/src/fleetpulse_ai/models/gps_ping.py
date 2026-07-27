
from dataclasses import dataclass


@dataclass
class GpsPing:
    """Represents a GPS ping from a vehicle."""
    
    def __init__(self, latitude: float, longitude: float, speed_kmh: float, heading_degrees: float, timestamp: str):
        self.latitude = latitude
        self.longitude = longitude
        self.speed_kmh = speed_kmh
        self.heading_degrees = heading_degrees
        self.timestamp = timestamp