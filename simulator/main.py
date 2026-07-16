import asyncio
import json
from fleetpulse.mqtt_publisher import MQTTMockPublisher, MQTTPublisher
from fleetpulse.driver_simulator import DriverSimulator
from fleetpulse.drivers import DriverConfig
from utils.route_preprocessor import resample_geojson


class FleetPulseSimulator:

    def __init__(self):
        self.broker = "localhost"
        self.port = 1883

    async def run(self, path:str, route_id:str):
        print("Starting FleetPulse Simulator...")

        driver_config = DriverConfig(
            driver_id="driver_001", 
            vehicle_type="motorcycle",
            name="John Doe",
            route_id=route_id,
            start_offset_seconds=0.0
            )
        route = resample_geojson(path, route_id)
        print(f"Loaded route {route_id} with {len(route.points)} points, total distance: {route.total_distance:.2f} meters.")
        
        async with MQTTPublisher(self.broker, self.port) as publisher:
        #async with MQTTMockPublisher() as publisher:
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

        #json.dump(publisher.published_messages, open("data/recoleta_route_sample_output.json", "w"), indent=2)

if __name__ == '__main__':
    print(" * Running FleetPulse Simulator...")
    path = "data/routes/raw/recoleta_route_sample.geojson"
    simulator = FleetPulseSimulator()
    asyncio.run(simulator.run(path, route_id="recoleta_route_sample"))    