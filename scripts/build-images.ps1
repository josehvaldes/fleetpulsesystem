param(
    [string]$Version = "1.0"
)

$ErrorActionPreference = "Stop"

docker build -t "fleetpulse-signalrhub:$Version" `
    -f .\FleetPulse.SignalRHub\FleetPulse.SignalRHub\Dockerfile `
    .\FleetPulse.SignalRHub\

docker build -t "fleetpulse-dbwriter:$Version" `
    -f .\FleetPulse.DbWriter\FleetPulse.DbWriter\Dockerfile `
    .\FleetPulse.DbWriter\

docker build -t "fleetpulse-ai-worker:$Version" `
    -f .\ai-worker\docker\Dockerfile `
    .\ai-worker\

docker build -t "fleetpulse-api:$Version" `
    -f .\FleetPulse.Api\FleetPulse.Api\Dockerfile `
    .\FleetPulse.Api\

docker build -t "fleetpulse-yarpproxy:$Version" `
    -f .\FleetPulse.YarpProxy\FleetPulse.YarpProxy\Dockerfile `
    .\FleetPulse.YarpProxy\