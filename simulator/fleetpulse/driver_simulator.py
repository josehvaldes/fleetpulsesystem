import asyncio
import math
import random
from datetime import datetime, timezone
from fleetpulse.mqtt_publisher import MQTTPublisherInterface
from fleetpulse.drivers import DriverConfig
from utils.processed_route import ProcessedRoute


class DriverSimulator:
    # Base speed ranges for motorcycle delivery (km/h)
    BASE_SPEED_RANGE = (20.0, 45.0)
    
    def __init__(
        self,
        config: DriverConfig,
        route: ProcessedRoute,
        publisher: MQTTPublisherInterface,
        sim_config: dict,
    ):
        self.config = config
        self.route = route
        self.publisher = publisher
        self.sim_config = sim_config
        
        # Driver's "personality" - base speed
        self.base_speed_kmh = random.uniform(*self.BASE_SPEED_RANGE)
        self.current_speed_kmh = self.base_speed_kmh
        self.target_speed_kmh = self.base_speed_kmh
        
        # Position tracking
        self.distance_along_route = 0.0  # meters
        self.current_lat = route.points[0][0]
        self.current_lng = route.points[0][1]
        self.heading = 0.0
        
        # State
        self.status = "moving"  # moving, decelerating, stopped, accelerating
        self.stop_timer = 0.0
        self.running = False

  
    def _get_point_at_distance(self, distance: float) -> tuple[float, float, float, int]:
        """
        Get (lat, lng, heading, point_index) at given distance along route.
        Interpolates between resampled points if needed.
        """
        points = self.route.points
        distances = self.route.distances
        
        # Find the segment
        idx = 0
        while idx < len(distances) - 1 and distances[idx + 1] < distance:
            idx += 1
        
        if idx >= len(points) - 1:
            # At or past the end
            return points[-1][0], points[-1][1], self.heading, len(points) - 1
        
        # Interpolate within segment
        seg_start_dist = distances[idx]
        seg_end_dist = distances[idx + 1]
        seg_length = seg_end_dist - seg_start_dist
        
        if seg_length == 0:
            return points[idx][0], points[idx][1], self.heading, idx
        
        fraction = (distance - seg_start_dist) / seg_length
        fraction = max(0.0, min(1.0, fraction))
        
        lat = points[idx][0] + (points[idx + 1][0] - points[idx][0]) * fraction
        lng = points[idx][1] + (points[idx + 1][1] - points[idx][1]) * fraction
        
        # Calculate heading from this segment
        dlng = math.radians(points[idx + 1][1] - points[idx][1])
        lat1_r = math.radians(points[idx][0])
        lat2_r = math.radians(points[idx + 1][0])
        x = math.sin(dlng) * math.cos(lat2_r)
        y = math.cos(lat1_r) * math.sin(lat2_r) - math.sin(lat1_r) * math.cos(lat2_r) * math.cos(dlng)
        heading = (math.degrees(math.atan2(x, y)) + 360) % 360
        
        return lat, lng, heading, idx
    
    def _detect_curvature(self, point_idx: int) -> float:
        """
        Detect curve sharpness by comparing headings of adjacent segments.
        Returns curvature value 0.0 (straight) to 1.0 (sharp turn).
        """
        if point_idx < 1 or point_idx >= len(self.route.points) - 1:
            return 0.0
        
        # Get headings for previous and next segments
        def segment_heading(i: int) -> float:
            p1 = self.route.points[i]
            p2 = self.route.points[i + 1]
            dlng = math.radians(p2[1] - p1[1])
            lat1_r = math.radians(p1[0])
            lat2_r = math.radians(p2[0])
            x = math.sin(dlng) * math.cos(lat2_r)
            y = math.cos(lat1_r) * math.sin(lat2_r) - math.sin(lat1_r) * math.cos(lat2_r) * math.cos(dlng)
            return math.degrees(math.atan2(x, y))
        
        h1 = segment_heading(point_idx - 1)
        h2 = segment_heading(point_idx)
        
        # Angle difference (0-180 degrees)
        angle_diff = abs(h2 - h1) % 360
        if angle_diff > 180:
            angle_diff = 360 - angle_diff
        
        # Normalize to 0-1 (30+ degrees is a sharp turn for a motorcycle)
        return min(angle_diff / 30.0, 1.0)
    
    def _update_speed(self, point_idx: float, dt: float):
        """
        Dynamically adjust speed based on:
        - Curvature ahead
        - Random events (traffic, delivery stops)
        - Smooth acceleration/deceleration
        """
        curvature = self._detect_curvature(int(point_idx))
        
        # Random stop event (traffic light, delivery, etc.)
        if self.status == "moving" and random.random() < 0.002:  # ~0.2% chance per tick
            self.status = "decelerating"
            self.target_speed_kmh = 0.0
            self.stop_timer = random.uniform(5.0, 30.0)  # 5-30 second stop
        
        # Calculate target speed based on curvature
        if self.status == "moving" or self.status == "accelerating":
            # Slow down for curves
            curve_factor = 1.0 - (curvature * 0.6)  # 60% speed reduction on sharp turns
            self.target_speed_kmh = self.base_speed_kmh * curve_factor
            self.status = "accelerating" if self.current_speed_kmh < self.target_speed_kmh else "moving"
        
        # Handle stopped state
        if self.status == "stopped":
            self.stop_timer -= dt
            if self.stop_timer <= 0:
                self.status = "accelerating"
                self.target_speed_kmh = self.base_speed_kmh
        
        # Smooth speed transition (realistic acceleration)
        if self.status == "decelerating":
            # Brake harder than accelerate
            decel_rate = 40.0  # km/h per second (aggressive braking)
            self.current_speed_kmh = max(0, self.current_speed_kmh - decel_rate * dt)
            if self.current_speed_kmh <= 0:
                self.current_speed_kmh = 0
                self.status = "stopped"
        
        elif self.status == "accelerating":
            accel_rate = 15.0  # km/h per second (motorcycle acceleration)
            self.current_speed_kmh = min(
                self.target_speed_kmh, 
                self.current_speed_kmh + accel_rate * dt
            )
            if self.current_speed_kmh >= self.target_speed_kmh:
                self.status = "moving"
        
        # Small random speed variation (wind, traffic flow)
        if self.status == "moving":
            self.current_speed_kmh += random.gauss(0, 0.5)
            self.current_speed_kmh = max(5, min(self.base_speed_kmh * 1.1, self.current_speed_kmh))
    
    def _add_gps_noise(self, lat: float, lng: float) -> tuple[float, float]:
        """Realistic GPS jitter"""
        noise_m = random.gauss(0, self.sim_config.get("gps_noise_meters", 4.0))
        lat_noise = (noise_m / 111320) 
        lng_noise = (noise_m / (111320 * math.cos(math.radians(lat))))
        return lat + lat_noise, lng + lng_noise
    

    async def run(self):
        self.running = True
        
        # Staggered start
        if self.config.start_offset_seconds > 0:
            await asyncio.sleep(self.config.start_offset_seconds)
        
        time_accel = self.sim_config.get("time_acceleration", 1.0)
        ping_interval = self.sim_config.get("base_ping_interval_seconds", 1.0)
        jitter = self.sim_config.get("ping_interval_jitter_seconds", 0.2)

        print(f"Driver {self.config.driver_id} starting simulation with base speed {self.base_speed_kmh:.1f} km/h.")        

        while self.running:
            # Calculate actual ping interval with jitter
            actual_interval = ping_interval + random.uniform(-jitter, jitter)
            
            # Simulation time step (accelerated)
            sim_dt = actual_interval * time_accel
            
            # Update speed based on current position
            point_idx = self._get_point_at_distance(self.distance_along_route)[3]
            self._update_speed(point_idx, sim_dt)
            
            # Calculate distance traveled this tick
            # speed_m/s = speed_kmh * 1000 / 3600
            distance_delta = (self.current_speed_kmh * 1000 / 3600) * sim_dt
            self.distance_along_route += distance_delta
            
            # Handle route end
            
            if self.distance_along_route >= self.route.total_distance:
                print(f"Driver {self.config.driver_id} reached end of route. Stop")
                self.running = False
            
            # Get new position
            lat, lng, heading, _ = self._get_point_at_distance(self.distance_along_route)
            lat, lng = self._add_gps_noise(lat, lng)
            self.heading = heading
            print(f"Driver {self.config.driver_id} distance along route: {self.distance_along_route:.2f} meters | Pos: ({lat:.6f}, {lng:.6f}) | Speed: {self.current_speed_kmh:.1f} km/h | Status: {self.status}")

            # Build and publish message
            message = {
                "driver_id": self.config.driver_id,
                "timestamp": datetime.now(timezone.utc).isoformat(),
                "latitude": round(lat, 6),
                "longitude": round(lng, 6),
                "speed_kmh": round(self.current_speed_kmh, 1),
                "heading_degrees": round(self.heading, 1),
                "accuracy_meters": round(abs(random.gauss(4.0, 1.5)), 1),
                "status": self.status if self.status != "decelerating" else "moving",
                "vehicle_type": self.config.vehicle_type,
            }
            
            await self.publisher.publish(message)
            
            # Sleep for real-time interval
            await asyncio.sleep(actual_interval)
        
        print(f"Driver {self.config.driver_id} simulation stopped.")
    
    def stop(self):
        self.running = False
        print(f"Driver {self.config.driver_id} simulation stopping...")    