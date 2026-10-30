
import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig([
  {
  input: '../FleetPulse.Api/FleetPulse.Api/openapi/FleetPulse.Api.json',
  output: {
    path: 'src/generated/api',
  },
},
  {
  input: '../FleetPulse.SignalRHub/FleetPulse.SignalRHub/openapi/FleetPulse.SignalRHub.json',
  output: {
    path: 'src/generated/signalrhub',
  },
}
]);
