import asyncio
from fleetpulse.mqtt_publisher import TelemetryPublisher
from fleetpulse.drivers import Coordinate, Driver, DriverStatus

drivers = [
    # Example driver data; in a real scenario, this would be dynamic or loaded from a source
    Driver(
        driver_id="driver_1",
        route=[Coordinate(38.7749, -128.4194), Coordinate(38.8044, -128.2711)],
        current_position=Coordinate(38.7749, -128.4194),
        speed_factor=1.0,
        status=DriverStatus.MOVING
    ),]

class FleetPulseSimulator:

    def __init__(self):
        self.broker = "localhost"
        self.port = 1883

    async def run(self):
        
        
        print("Starting FleetPulse Simulator...")
        # Main loop to simulate driver behavior and publish GPS pings
        async with TelemetryPublisher(self.broker,self.port) as publisher:
            for driver in drivers:
                # Simulate driver movement along the route
                print(f"Publishing GPS ping for {driver.driver_id} at position ({driver.current_position.latitude}, {driver.current_position.longitude})")
                await publisher.publish_gps(driver)
            

if __name__ == '__main__':
    simulator = FleetPulseSimulator()
    asyncio.run(simulator.run())    