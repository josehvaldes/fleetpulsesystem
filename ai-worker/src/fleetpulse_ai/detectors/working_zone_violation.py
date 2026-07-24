# detectors/working_zone_violation.py
from fleetpulse_ai.detectors.base_detector import BaseDetector
from fleetpulse_ai.events.violation_event import ViolationEvent
from fleetpulse_ai.models.gps_ping import GpsPing
from shapely.geometry import Point, Polygon

class WorkingZoneViolationDetector(BaseDetector):
    """Detects when a driver exits their assigned working zone polygon."""
    
    def __init__(self, driver_zones: dict[str, Polygon]):
        # Loaded from data/driver_zones.json
        self.zones = driver_zones
        
    async def analyze(self, driver_id: str, history: list[GpsPing]) -> ViolationEvent | None:
        if len(history) < 2:
            return None
            
        prev, curr = history[-2], history[-1]
        zone = self.zones.get(driver_id)
        
        if not zone:
            print(f"No working zone found for driver {driver_id}.")
            return None
            
        was_inside = zone.contains(Point(prev.longitude, prev.latitude))
        is_outside = not zone.contains(Point(curr.longitude, curr.latitude))
        print (f"Driver {driver_id} | Was inside: {was_inside} | Is outside: {is_outside}")

        if was_inside and is_outside:
            return ViolationEvent(
                driver_id=driver_id,
                exit_location={"lat": curr.latitude, "lng": curr.longitude},
                exit_speed=curr.speed_kmh,
                exit_heading=curr.heading_degrees,
                exit_time=curr.timestamp,
                zone_name=zone.name,
                zone_type="working_zone"
            )
        return None