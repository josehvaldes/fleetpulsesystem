import asyncio
import json
import os

from dotenv import load_dotenv
from pathlib import Path
load_dotenv(Path(__file__).resolve().parent / ".env")

from fleetpulse.mqtt_publisher import MQTTMockPublisher, MQTTPublisher, MQTTPublisherInterface
from fleetpulse.driver_simulator import DriverSimulator
from fleetpulse.drivers import DriverConfig
from utils.logging_config import get_logger, setup_logging
from utils.processed_route import ProcessedRoute
from utils.route_preprocessor import resample_geojson

from opentelemetry import trace
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor, ConsoleSpanExporter
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.sdk.trace.sampling import TraceIdRatioBased, ALWAYS_ON

MOCK_MODE = os.getenv("MOCK_MODE", "true").lower() == "true"
OTEL_SERVER = os.getenv("OTEL_SERVER", "http://localhost:4317")  # Default to localhost if not set
CONSOLE_TRACE_EXPORTER = os.getenv("CONSOLE_TRACE_EXPORTER", "false").lower() == "true"
OTLP_EXPORTER = os.getenv("OTLP_EXPORTER", "true").lower() == "true"
ENVIRONMENT = os.getenv("ENVIRONMENT", "dev")

setup_logging(
    log_level="INFO",
    log_file=None,
    log_to_console=True,
    service_name="fleetpulse-simulator",
)

logger = get_logger(__name__)

def setup_tracing() -> None:
    resource = Resource.create({
        "service.name": "fleetpulse.simulator",
        "service.version": "0.1.0",
        "deployment.environment": ENVIRONMENT,
    })

    if ENVIRONMENT == "development":
        sampler= ALWAYS_ON  # Sample all traces in development
    else:
        sampler = TraceIdRatioBased(1.0)  # Sample all traces

    provider = TracerProvider(resource=resource, sampler=sampler)  # Sample all traces

    if CONSOLE_TRACE_EXPORTER:
        provider.add_span_processor(BatchSpanProcessor(ConsoleSpanExporter()))

    if OTLP_EXPORTER:
        provider.add_span_processor(BatchSpanProcessor(
            OTLPSpanExporter(endpoint=OTEL_SERVER, insecure=True)
        ))
    trace.set_tracer_provider(provider)

setup_tracing()

class FleetPulseSimulator:

    def __init__(self):
        self.broker = "localhost"
        self.port = 1883

    async def load_and_run(self, path:str, route_id:str, driver_config: DriverConfig):

        logger.info("Starting FleetPulse Simulator...")
        with open(path) as f:
            route = json.load(f, object_hook=lambda d: ProcessedRoute(**d))

            logger.info(f"Loaded route {route_id} with {len(route.points)} points, total distance: {route.total_distance:.2f} meters.")
            async with MQTTMockPublisher() as publisher:
                simulator = DriverSimulator(
                    config=driver_config,
                    route=route,
                    publisher=publisher,
                    sim_config={"update_interval": 1.0}
                )
    
                task = asyncio.create_task(simulator.run(), name="DriverSimulatorTask")
                logger.info("FleetPulse Simulator is running. Press Ctrl+C to stop.")
                # wait for the simulation to complete
                await task
            json.dump(publisher.published_messages, open("data/recoleta_route_sample_output.json", "w"), indent=2)



    async def run_org(self, path:str, route_id:str, driver_config: DriverConfig):
        logger.info(f"Starting FleetPulse Simulator for driver {driver_config.driver_id} on route {route_id}...")

        #regenerate the processed route from the raw geojson
        route = resample_geojson(path, route_id)
        logger.info(f" - Loaded route {route_id} with {len(route.points)} points, total distance: {route.total_distance:.2f} meters.")
        
        async with MQTTPublisher(self.broker, self.port) as publisher:
            simulator = DriverSimulator(
                config=driver_config,
                route=route,
                publisher=publisher,
                sim_config={"update_interval": 1.0}
            )
            await simulator.run()
    
            #task = asyncio.create_task(simulator.run(), name="DriverSimulatorTask")
            #print(" - FleetPulse Simulator is running. Press Ctrl+C to stop.")
            # wait for the simulation to complete
            #await task

    async def run (self, driver_configs: dict[str, DriverConfig], publisher: MQTTPublisherInterface):
        driver_route_file = "data/drivers_routes.json"
        tasks = []
        with open(driver_route_file) as f:
            driver_routes_data = json.load(f)
            for data in driver_routes_data:
                if data.get("driver_id") in driver_configs:
                    driver_config = driver_configs[data.get("driver_id")]
                    route_path = f"data/routes/raw/{data.get('route_id')}.geojson"

                    route_id = data.get('route_id')
                    logger.info(f"Starting FleetPulse Simulator for driver {driver_config.driver_id} on route {route_id}...")
                    
                    #regenerate the processed route from the raw geojson
                    route = resample_geojson(route_path, route_id)
                    logger.info(f" - Loaded route {route_id} with {len(route.points)} points, total distance: {route.total_distance:.2f} meters.")
                    
                    simulator = DriverSimulator(
                            config=driver_config,
                            route=route,
                            publisher=publisher,
                            sim_config={"update_interval": 1.0}
                        )
                    tasks.append(simulator.run())

                else:
                    logger.warning(f"Driver ID {data.get('driver_id')} not found in drivers.json. Skipping.")
        return tasks

    async def exec(self):
        logger.info("Starting FleetPulse Simulator...")
        drivers_file = "data/drivers.json"
        driver_configs = dict[str, DriverConfig]()

        with open(drivers_file) as f:
            drivers_data = json.load(f)
            for driver_data in drivers_data:
                driver_config = DriverConfig(**driver_data)
                driver_configs[driver_config.driver_id] = driver_config


        if MOCK_MODE:
            async with MQTTMockPublisher() as publisher:
                task = await self.run(driver_configs, publisher)
                await asyncio.gather(*task)
        else:
            async with MQTTPublisher(self.broker, self.port) as publisher:
                task = await self.run(driver_configs, publisher)
                await asyncio.gather(*task)



if __name__ == '__main__':
    logger.info(f" * Running FleetPulse Simulator. Mockup Mode: {MOCK_MODE}...")
    
    simulator = FleetPulseSimulator()
    asyncio.run(simulator.exec())

    # driver_config = DriverConfig(
    #     driver_id="driver_001",
    #     vehicle_type="motorcycle",
    #     behavior_profile="normal",
    #     name="John Doe",
    #     route_id="route_cross_zone",
    #     start_offset_seconds=0.0
    #     )
    # path = "data/routes/raw/route_cross_zone.geojson"
    # asyncio.run(simulator.run(path, route_id="route_cross_zone", driver_config=driver_config))
    # path_processed = "data/routes/processed/recoleta_route_sample_output.json"
    # asyncio.run(simulator.load_and_run(path_processed, route_id="recoleta_route_sample", driver_config=driver_config))
    