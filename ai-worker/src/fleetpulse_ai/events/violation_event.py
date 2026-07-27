from dataclasses import asdict, dataclass


@dataclass
class ViolationEvent:
    """Represents a violation event when a driver exits their assigned working zone."""
    
    driver_id: str
    exit_location: dict[str, float]
    exit_speed: float
    exit_heading: float
    exit_time: str
    zone_name: str
    zone_type: str

    def to_dict(self) -> dict:
        return asdict(self)