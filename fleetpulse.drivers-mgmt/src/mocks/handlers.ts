import { http, HttpResponse } from 'msw';

import { LastestDriverStateResponse } from '@app/generated/api';
import { environment } from '@env/environment';

const mockDrivers: LastestDriverStateResponse[] = [
    {
        driverId: "test-01",
        speed: 50,
        lastSeen: "2026-09-21T00:00:00Z",
        status: "moving",
        latitude: -17.98650,
        longitude: -14.98754,
    },
    {
      driverId: "test-02",
      speed: 0,
      lastSeen: "2026-09-21T00:00:00Z",
      status: "stopped",
      latitude: -18.12345,
      longitude: -15.12345,
    },
    {
      driverId: "test-03",
      speed: 30,
      lastSeen: "2026-09-21T00:00:00Z",
      status: "moving",
      latitude: -19.54321,
      longitude: -16.54321,
    }
]

export const handlers = [
  http.get(`${environment.apiUrl}/v1/drivers`, () => {
    console.log("Fetching mock drivers data");
    return HttpResponse.json(mockDrivers);
  }),
];