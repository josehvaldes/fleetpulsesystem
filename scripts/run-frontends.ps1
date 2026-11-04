# script to start the frontend apps in dev mode

concurrently `
    --names "frontend,alerts,drivers" `
    --prefix-colors "blue,green,magenta" `
    "npm run dev --prefix ..\frontend\fleetpulse.alert-mgmt" `
    "npm run dev --prefix ..\frontend\fleetpulse.frontend" `
    "npm run start --prefix ..\frontend\fleetpulse.drivers-mgmt"
