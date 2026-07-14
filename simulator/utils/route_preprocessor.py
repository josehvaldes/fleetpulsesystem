import json
import math
from pathlib import Path
from utils.processed_route import ProcessedRoute

def haversine(lat1: float, lng1: float, lat2: float, lng2: float) -> float:
    """Distance in meters between two GPS points"""
    R = 6371000
    phi1, phi2 = math.radians(lat1), math.radians(lat2)
    dphi = math.radians(lat2 - lat1)
    dlambda = math.radians(lng2 - lng1)
    a = math.sin(dphi/2)**2 + math.cos(phi1)*math.cos(phi2)*math.sin(dlambda/2)**2
    return R * 2 * math.atan2(math.sqrt(a), math.sqrt(1-a))


def interpolate_point(
    lat1: float, lng1: float, 
    lat2: float, lng2: float, 
    fraction: float
) -> tuple[float, float]:
    """Linear interpolation between two points"""
    return (
        lat1 + (lat2 - lat1) * fraction,
        lng1 + (lng2 - lng1) * fraction
    )


def calculate_bearing(lat1: float, lng1: float, lat2: float, lng2: float) -> float:
    """Bearing in degrees from point 1 to point 2"""
    dlng = math.radians(lng2 - lng1)
    lat1_r, lat2_r = math.radians(lat1), math.radians(lat2)
    x = math.sin(dlng) * math.cos(lat2_r)
    y = math.cos(lat1_r) * math.sin(lat2_r) - math.sin(lat1_r) * math.cos(lat2_r) * math.cos(dlng)
    return (math.degrees(math.atan2(x, y)) + 360) % 360


def resample_geojson_delta(
    geojson_path: Path, 
    route_id: str,
    interval_meters: float = 10.0
) -> ProcessedRoute:
    with open(geojson_path) as f:
        data = json.load(f)
    
    raw_coords = data["geometry"]["coordinates"]  # [lng, lat]
    
    # Convert to (lat, lng) and calculate segment distances
    segments: list[tuple[tuple[float, float], float]] = []
    total_raw_distance = 0.0
    
    for i in range(len(raw_coords) - 1):
        lng1, lat1 = raw_coords[i]
        lng2, lat2 = raw_coords[i + 1]
        dist = haversine(lat1, lng1, lat2, lng2)
        segments.append(((lat1, lng1), dist))
        total_raw_distance += dist
    

    return ProcessedRoute(
        route_id=route_id,
        points=[(lat, lng) for (lat, lng), _ in segments] ,
        delta_distances=[0] + [dist for _, dist in segments],
        distances=[sum([dist for _, dist in segments[:i]]) for i in range(len(segments))] + [total_raw_distance],
        total_distance=total_raw_distance,
        resample_interval=interval_meters
    )


def resample_geojson(
    geojson_path: Path, 
    route_id: str,
    interval_meters: float = 10.0
) -> ProcessedRoute:
    """
    Convert raw GeoJSON to equidistant points.
    
    interval_meters: distance between resampled points (10m = good balance)
    """
    with open(geojson_path) as f:
        data = json.load(f)
    
    raw_coords = data["geometry"]["coordinates"]  # [lng, lat]
    
    # Convert to (lat, lng) and calculate segment distances
    segments: list[tuple[tuple[float, float], float]] = []
    total_raw_distance = 0.0
    
    for i in range(len(raw_coords) - 1):
        lng1, lat1 = raw_coords[i]
        lng2, lat2 = raw_coords[i + 1]
        dist = haversine(lat1, lng1, lat2, lng2)
        segments.append(((lat1, lng1), dist))
        total_raw_distance += dist
    
    # Add final point
    last_lng, last_lat = raw_coords[-1]
    
    # Resample to equidistant points
    resampled_points: list[tuple[float, float]] = []
    cumulative_distances: list[float] = []
    
    current_distance = 0.0
    segment_idx = 0
    position_in_segment = 0.0  # How far into current segment (meters)
    
    while current_distance <= total_raw_distance:
        # Find which segment we're in
        while segment_idx < len(segments) - 1 and position_in_segment >= segments[segment_idx][1]:
            position_in_segment -= segments[segment_idx][1]
            segment_idx += 1
        
        if segment_idx >= len(segments):
            break
        
        # Calculate position within this segment
        seg_point, seg_length = segments[segment_idx]
        next_lat, next_lng = raw_coords[segment_idx + 1][1], raw_coords[segment_idx + 1][0]
        
        if seg_length > 0:
            fraction = position_in_segment / seg_length
            lat, lng = interpolate_point(seg_point[0], seg_point[1], next_lat, next_lng, fraction)
        else:
            lat, lng = seg_point
        
        resampled_points.append((lat, lng))
        cumulative_distances.append(current_distance)
        
        current_distance += interval_meters
        position_in_segment += interval_meters
    
    # Ensure we include the exact end point
    if resampled_points[-1] != (last_lat, last_lng):
        resampled_points.append((last_lat, last_lng))
        cumulative_distances.append(total_raw_distance)


    return ProcessedRoute(
        route_id=route_id,
        points=resampled_points,
        distances=cumulative_distances,
        total_distance=total_raw_distance,
        resample_interval=interval_meters,
        delta_distances=[0] + [cumulative_distances[i] - cumulative_distances[i-1] for i in range(1, len(cumulative_distances))]
    )


def preprocess_and_save(geojson_path: Path, output_path: Path, route_id: str):
    """Convenience function to preprocess and save"""
    route = resample_geojson_delta(geojson_path, route_id, interval_meters=10.0)
    
    with open(output_path, 'w') as f:
        json.dump(route.to_dict(), f, indent=2)
    
    print(f"Processed: {geojson_path.name}")
    print(f"  Raw distance: {route.total_distance:.0f}m")
    print(f"  Resampled points: {len(route.points)} (every {route.resample_interval}m)")
    print(f"  Output: {output_path}")


if __name__ == "__main__":
    script_dir = Path(__file__).parent.parent  # simulator/
    routes_dir = (script_dir / "data/routes/raw").resolve()
    output_dir = (script_dir / "data/routes/processed").resolve()
    output_dir.mkdir(parents=True, exist_ok=True)
        
    for geojson_file in routes_dir.glob("*.geojson"):
        route_id = geojson_file.stem
        output_path = output_dir / f"{route_id}_delta.json"
        preprocess_and_save(geojson_file, output_path, route_id)