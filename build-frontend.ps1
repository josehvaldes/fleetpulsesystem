param(
    [string]$Version = "1.0"
)

$ErrorActionPreference = "Stop"

docker build -t "fleetpulse-frontend:$Version" `
    -f .\fleetpulse.frontend\Dockerfile `
    .\fleetpulse.frontend\

docker build -t "fleetpulse.alert-mgmt:$Version" `
    -f .\fleetpulse.alert-mgmt\Dockerfile `
    .\fleetpulse.alert-mgmt\

docker build -t "fleetpulse.drivers-mgmt:$Version" `
    -f .\fleetpulse.drivers-mgmt\Dockerfile `
    .\fleetpulse.drivers-mgmt\