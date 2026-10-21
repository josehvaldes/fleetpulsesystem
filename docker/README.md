# Docker composition files

- *docker-compose-aggregated.yml* runs all the container at once. 
  This file uses a bridged 'fleetpulse_network' network.

If you need to run specific services, run the each commands below as needed:
All these services docker files use an external "redpanda_network" network so that it doesn't have conflicts with the bridged network of the aggregated file. Create the "redpanda_network" network in your local system before running these commands:

 docker compose -f .\docker-compose-postgresql.yml up -d ; `
 docker compose -f .\docker-compose-redpanda.yml up -d ; `
 docker compose -f .\docker-compose-observability.yml up -d ; `
 docker compose -f .\docker-compose-services.yml up -d ; `
 docker compose -f .\docker-compose-prometheus.yml up -d ; ` 
 docker compose -f .\docker-compose-frontend.yml up -d


- *docker-compose-mockflethub.yml* and the 'FleetPulse.MockFleetHub' project is a mockup web app that emulated the signaRhub connection and API. It is just a tool to help testing the UI.