import { http, HttpResponse } from 'msw';

import { DriverStateResponse } from '@app/core/contracts/driverstateresponse';

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
]

export const handlers = [
  http.get('/api/v1/drivers', () => {
    return HttpResponse.json(mockDrivers);
  }),
];