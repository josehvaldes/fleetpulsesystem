import asyncio
import json
import logging
from pathlib import Path
from typing import Awaitable, Callable

from shapely.geometry import Polygon, shape

from fleetpulse_ai.agents.alarm_analyzer_agent import AlarmAnalyzerAgent
from fleetpulse_ai.detectors.working_zone_violation import WorkingZoneViolationDetector
from fleetpulse_ai.managers.kafka_consumer import KafkaConsumer
from fleetpulse_ai.managers.alert_manager import AlertManager
from fleetpulse_ai.models.alert_event import AlertEvent
from fleetpulse_ai.models.gps_ping import GpsPing
from fleetpulse_ai.settings import settings

REPO_ROOT = Path(__file__).resolve().parents[2]
DATA_DIR = REPO_ROOT / "data"

MAX_HISTORY_LENGTH = 10


def create_ai_worker_handler(
    detector: WorkingZoneViolationDetector,
    manager: AlertManager,
    agent: AlarmAnalyzerAgent
) -> Callable[[dict], Awaitable[None]]:
    driver_history: dict[str, list[GpsPing]] = {}

    async def ai_worker_handler(message: dict) -> None:
        """
        Process a GPS ping message with the detector and alert manager bound for the worker lifetime.
        """

        point = GpsPing(
            latitude=float(message["latitude"]),
            longitude=float(message["longitude"]),
            speed_kmh=float(message["speed_kmh"]),
            heading_degrees=float(message["heading_degrees"]),
            timestamp=message["timestamp"],
        )

        driver_id = message["driver_id"]
        history = driver_history.setdefault(driver_id, [])

        if len(history) >= MAX_HISTORY_LENGTH:
            history.pop(0)

        history.append(point)

        if len(history) < 2:
            print(f"Not enough data to analyze for driver {driver_id}. Current history length: {len(history)}")
            return

        violation_event = await detector.analyze(driver_id, history)
        if violation_event:
            print(f"Violation detected for driver {driver_id}: {violation_event.to_dict()}")
            agent_response = await agent.analyze(driver_id, history)
            alert = AlertEvent()
            alert.update_alert_event(agent_response, violation_event)
            await manager.handle_alert(alert)
            

    return ai_worker_handler


def load_driver_zones(data_dir: Path | None = None) -> dict[str, tuple[str | None, Polygon | None]]:
    """
    Load driver zones from JSON and GeoJSON files stored in the repository data directory.
    """

    resolved_data_dir = data_dir or DATA_DIR

    #scan the resolved_data_dir for allowed_zones and driver_zones_mapping.json
    if not resolved_data_dir.exists():
        raise FileNotFoundError(f"Data directory '{resolved_data_dir}' does not exist.")

    allowed_zones_dir = resolved_data_dir / "allowed_zones"
    if not allowed_zones_dir.exists():
        raise FileNotFoundError(f"Allowed zones directory '{allowed_zones_dir}' does not exist.")

    polygons = {}
    for file in allowed_zones_dir.glob("*.geojson"):
        path_id = file.stem

        with file.open(encoding="utf-8") as f:
            data = json.load(f)
        polygons[path_id] = shape(data["geometry"])
        
    driving_zones: dict[str, tuple[str | None, Polygon | None]] = {}

    with (resolved_data_dir / "driver_zones_mapping.json").open(encoding="utf-8") as f:
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

async def main():
    print("Starting FleetPulse AI...")
    print("Settings:", settings.title)
    
    driving_zones = load_driver_zones()
    working_zone_detector = WorkingZoneViolationDetector(driver_zones=driving_zones)

    bootstrap_servers = settings.kafka_bootstrap_servers
    group_id = settings.kafka_group_id
    topics = [topic.strip() for topic in settings.kafka_topics.split(",") if topic.strip()]

    consumer = KafkaConsumer(
        bootstrap_servers=bootstrap_servers,
        group_id=group_id,
        topics=topics,
    )

    alarm_analyzer_agent = AlarmAnalyzerAgent(model_deployment=settings.azure_openai_model_deployment)

    async with AlertManager() as manager:
        message_handler = create_ai_worker_handler(working_zone_detector, manager, alarm_analyzer_agent)
        print("Starting consumer loop. Press Ctrl+C to exit.")
        await consumer.consume(message_handler=message_handler)


if __name__ == "__main__":
   logging.basicConfig(
           level=logging.INFO,
           format="%(asctime)s | %(name)s | %(levelname)s | %(message)s",
       )   
   asyncio.run(main())
