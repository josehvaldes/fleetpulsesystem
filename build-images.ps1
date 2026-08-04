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

docker build -t "fleetpulse-frontend:$Version" `
    -f .\fleetpulse.frontend\Dockerfile `
    .\fleetpulse.frontend\