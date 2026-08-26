param(
    [string]$Version = "1.0"
)

$ErrorActionPreference = "Stop"

docker build -t "fleetpulse-frontend:$Version" `
    -f .\fleetpulse.frontend\Dockerfile `
    .\fleetpulse.frontend\

docker build -t "fleetpulse-mockfleethub:$Version" `
    -f .\FleetPulse.MockFleetHub\Dockerfile `
    .\FleetPulse.MockFleetHub\
