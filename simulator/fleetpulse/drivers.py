from dataclasses import dataclass
from enum import Enum

class DriverStatus(Enum):
    MOVING = "moving"
    STOPPED = "stopped"
    IDLE = "idle"

@dataclass
class Coordinate:
    latitude: float
    longitude: float
    
@dataclass
class Driver:
    driver_id: str
    route: list[Coordinate]
    current_position: Coordinate
    speed_factor: float  # 0.5 = slow, 1.0 = normal, 1.5 = fast
    status: DriverStatus  # moving, stopped, idle
