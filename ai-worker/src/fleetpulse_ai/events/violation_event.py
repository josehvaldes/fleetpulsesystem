class ViolationEvent:
    """Represents a violation event when a driver exits their assigned working zone."""
    
    def __init__(
        self,
        driver_id: str,
        exit_location: dict[str, float],
        exit_speed: float,
        exit_heading: float,
        exit_time: int,
        zone_name: str,
        zone_type: str
    ):
        self.driver_id = driver_id
        self.exit_location = exit_location
        self.exit_speed = exit_speed
        self.exit_heading = exit_heading
        self.exit_time = exit_time
        self.zone_name = zone_name
        self.zone_type = zone_type