# detectors/working_zone_violation.py
import structlog

from fleetpulse_ai.detectors.base_detector import BaseDetector
from fleetpulse_ai.events.violation_event import ViolationEvent
from fleetpulse_ai.models.gps_ping import GpsPing
from shapely.geometry import Point, Polygon
from fleetpulse_ai.logging_config import get_logger
logger = get_logger(__name__)

class WorkingZoneViolationDetector(BaseDetector):
    """Detects when a driver exits their assigned working zone polygon."""
    
    def __init__(self, driver_zones: dict[str, tuple[str, Polygon]]):
        # Loaded from data/driver_zones.json
        self.zones = driver_zones
        
    async def analyze(self, driver_id: str, history: list[GpsPing]) -> ViolationEvent | None:
        if len(history) < 2:
            return None
        
        prev, curr = history[-2], history[-1]
        zone_info = self.zones.get(driver_id)

        if not zone_info:
            logger.warning("driver_zone_not_configured", driver_id=driver_id)
            return None
        zone_name, zone = zone_info
            
        was_inside = zone.contains(Point(prev.longitude, prev.latitude))
        is_outside = not zone.contains(Point(curr.longitude, curr.latitude))

        if was_inside and is_outside:
            return ViolationEvent(
                driver_id=driver_id,
                exit_location={"latitude": curr.latitude, "longitude": curr.longitude},
                exit_speed=curr.speed_kmh,
                exit_heading=curr.heading_degrees,
                exit_time=curr.timestamp,
                zone_name=zone_name,
                zone_type="working_zone"
            )
        return None