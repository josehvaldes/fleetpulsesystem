import { http, HttpResponse } from 'msw';

import { DriverStateResponse } from '@app/core/contracts/driverstateresponse';
import { environment } from '@env/environment';

const mockDrivers: DriverStateResponse[] = [
    {
        id: "test-01",
        name: "Joe Doe",
        speed: 50,
        LastTimeSeen: "2026-09-21T00:00:00Z",
        status: "moving",
        latitude: -17.98650,
        longitude: -14.98754,
    },
    {
      id: "test-02",
      name: "Jane Smith",
      speed: 0,
      LastTimeSeen: "2026-09-21T00:00:00Z",
      status: "stopped",
      latitude: -18.12345,
      longitude: -15.12345,
    },
    {
      id: "test-03",
      name: "Alice Johnson",
      speed: 30,
      LastTimeSeen: "2026-09-21T00:00:00Z",
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