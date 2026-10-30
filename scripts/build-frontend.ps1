param(
    [string]$Version = "1.0"
)

$ErrorActionPreference = "Stop"

docker build -t "fleetpulse-frontend:$Version" `
    -f ..\frontend\fleetpulse.frontend\Dockerfile `
    ..\frontend\fleetpulse.frontend\

docker build -t "fleetpulse.alert-mgmt:$Version" `
    -f ..\frontend\fleetpulse.alert-mgmt\Dockerfile `
    ..\frontend\fleetpulse.alert-mgmt\

docker build -t "fleetpulse.drivers-mgmt:$Version" `
    -f ..\frontend\fleetpulse.drivers-mgmt\Dockerfile `
    ..\frontend\fleetpulse.drivers-mgmt\