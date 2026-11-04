
import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig(
 {
  input: '../FleetPulse.Api/FleetPulse.Api/openapi/FleetPulse.Api.json',
  output: {
    path: 'src/app/generated/api',
  },
} 
);
