import logging
import asyncio

from envs.cvenv.Lib import json
from shapely.geometry import Polygon, shape

from fleetpulse_ai.detectors.working_zone_violation import WorkingZoneViolationDetector
from fleetpulse_ai.events.violation_event import ViolationEvent
from fleetpulse_ai.kafka_consumer import KafkaConsumer
from fleetpulse_ai.models.gps_ping import GpsPing

MAX_HISTORY_LENGTH = 10
driver_history = {}
working_zone_detector = None  # This will be initialized later with actual driver zones

async def ai_worker_handler(message: dict) -> None:
    """
    Your AI worker logic goes here.
    `message` contains the deserialized Redpanda payload.
    """

    point = GpsPing(
        latitude= float(message['latitude']),
        longitude=float(message['longitude']),
        speed_kmh=float(message['speed_kmh']),
        heading_degrees=float(message['heading_degrees']),
        timestamp=message['timestamp']
    )

    driver_id = message['driver_id']
    if driver_id not in driver_history:
        driver_history[driver_id] = []

    #keep only the last 10 pings for each driver
    if len(driver_history[driver_id]) >= MAX_HISTORY_LENGTH:
        driver_history[driver_id].pop(0)
    
    driver_history[driver_id].append(point)

    if working_zone_detector:
        history = driver_history[driver_id]
        if len(history) >= 2:  # Ensure we have enough data points to analyze
            violation_event = await working_zone_detector.analyze(driver_id, history)
            if violation_event:
                print(f"Violation detected for driver {driver_id}: {violation_event.to_dict()}")
        else:
            print(f"Not enough data to analyze for driver {driver_id}. Current history length: {len(history)}")
        
def load_driver_zones() -> dict[str, tuple[str, Polygon]]:
    """
    Load driver zones from a JSON file or database.
    For this example, we'll return a hardcoded dictionary.
    """

    geojson_path1 = "..\\data\\allowed_zones\\zone_1_south.geojson"
    with open(geojson_path1) as f:
        data1 = json.load(f)

    geojson_path2 = "..\\data\\allowed_zones\\zone_2_north.geojson"
    with open(geojson_path2) as f:
        data2 = json.load(f)

    polygons = {
        "zone_1_south": shape(data1["geometry"]),
        "zone_2_north": shape(data2["geometry"]),
    }

    driving_zones = {}

    with open("..\\data\\driver_zones_mapping.json") as f:
        driver_zones_data = json.load(f)
        for mapping in driver_zones_data:
            driver_id = mapping["driver_id"]
            zone_name = mapping["zone"]
            if zone_name in polygons:
                driving_zones[driver_id] = (zone_name, polygons[zone_name])
            else:
                print(f"Warning: Zone '{zone_name}' for driver '{driver_id}' not found in polygons.")
                driving_zones[driver_id] = (None, None)  # or handle as needed


    return driving_zones

if __name__ == "__main__":
    print("Starting FleetPulse AI...")
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s | %(name)s | %(levelname)s | %(message)s",
    )

    driving_zones = load_driver_zones()
    for driver_id, zone in driving_zones.items():
        if zone[0] is not None:
            print(f"Loaded zone for driver {driver_id}: {zone}")
        else:
            print(f"No valid zone loaded for driver {driver_id}.")

    working_zone_detector = WorkingZoneViolationDetector(driver_zones=driving_zones)  # Load actual zones here

    CONSUMER = KafkaConsumer(
        bootstrap_servers="localhost:19092",
        group_id="ai-worker-group",
        topics=["gps-pings"],
    )
    
    print("Starting consumer loop. Press Ctrl+C to exit.")
    asyncio.run(CONSUMER.consume(message_handler=ai_worker_handler))
