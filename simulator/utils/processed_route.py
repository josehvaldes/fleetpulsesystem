
from dataclasses import dataclass

@dataclass
class ProcessedRoute:
    route_id: str
    points: list[tuple[float, float]]  # (lat, lng)
    distances: list[float]             # cumulative distance from start in meters
    delta_distances: list[float]       # distance between consecutive points in meters
    total_distance: float
    resample_interval: float           # meters between points
    
    def to_dict(self) -> dict:
        return {
            "route_id": self.route_id,
            "resample_interval_m": self.resample_interval,
            "total_distance_m": round(self.total_distance, 2),
            "points": [
                {"lat": lat, "lng": lng, "cum_dist_m": round(dist, 2), "delta_dist_m": round(delta, 2)}
                for (lat, lng), dist, delta in zip(self.points, self.distances, self.delta_distances)
            ]
        }
    