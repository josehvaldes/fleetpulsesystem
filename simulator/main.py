import asyncio
import json
import signal
from fleetpulse.mqtt_publisher import MQTTMockPublisher, MQTTPublisher
from fleetpulse.driver_simulator import DriverSimulator
from fleetpulse.drivers import DriverConfig
from utils.processed_route import ProcessedRoute
from utils.route_preprocessor import resample_geojson


class FleetPulseSimulator:

    def __init__(self):
        self.broker = "localhost"
        self.port = 1883

    async def load_and_run(self, path:str, route_id:str, driver_config: DriverConfig):

        print("Starting FleetPulse Simulator...")
        with open(path) as f:
            route = json.load(f, object_hook=lambda d: ProcessedRoute(**d))

            print(f"Loaded route {route_id} with {len(route.points)} points, total distance: {route.total_distance:.2f} meters.")
            async with MQTTMockPublisher() as publisher:
                simulator = DriverSimulator(
                    config=driver_config,
                    route=route,
                    publisher=publisher,
                    sim_config={"update_interval": 1.0}
                )
    
                task = asyncio.create_task(simulator.run(), name="DriverSimulatorTask")
                print("FleetPulse Simulator is running. Press Ctrl+C to stop.")
                # wait for the simulation to complete
                await task
            json.dump(publisher.published_messages, open("data/recoleta_route_sample_output.json", "w"), indent=2)

    async def run(self, path:str, route_id:str, driver_config: DriverConfig):
        print("Starting FleetPulse Simulator...")

        #regenerate the processed route from the raw geojson
        route = resample_geojson(path, route_id)
        print(f"Loaded route {route_id} with {len(route.points)} points, total distance: {route.total_distance:.2f} meters.")
        
        async with MQTTPublisher(self.broker, self.port) as publisher:
            simulator = DriverSimulator(
                config=driver_config,
                route=route,
                publisher=publisher,
                sim_config={"update_interval": 1.0}
            )

            task = asyncio.create_task(simulator.run(), name="DriverSimulatorTask")
            print("FleetPulse Simulator is running. Press Ctrl+C to stop.")
            # wait for the simulation to complete
            await task

        

if __name__ == '__main__':
    print(" * Running FleetPulse Simulator...")
    
    driver_config = DriverConfig(
            driver_id="driver_001",
            vehicle_type="motorcycle",
            name="John Doe",
            route_id="route_cross_zone",
            start_offset_seconds=0.0
            )
    simulator = FleetPulseSimulator()

    path = "data/routes/raw/route_cross_zone.geojson"
    asyncio.run(simulator.run(path, route_id="route_cross_zone", driver_config=driver_config))

    # path_processed = "data/routes/processed/recoleta_route_sample_output.json"
    # asyncio.run(simulator.load_and_run(path_processed, route_id="recoleta_route_sample", driver_config=driver_config))
    