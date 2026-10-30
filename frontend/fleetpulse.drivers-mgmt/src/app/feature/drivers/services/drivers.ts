import { inject, Service } from '@angular/core';
import { DriversApiService } from '@app/core/api/drivers-api.service';
import { Driver } from '../models/driver';
import { map, Observable } from 'rxjs';

@Service()
export class Drivers {
    driverApi: DriversApiService = inject(DriversApiService);
    getDrivers():Observable<Driver[]> {
        const driversDto = this.driverApi.getDrivers();
        const drivers =  driversDto.pipe(
            map(apiDrivers => apiDrivers.map((apiDriver): Driver => (
                {
                id: apiDriver.driverId?? '',
                name: 'missing property',
                speed: apiDriver.speed ?? 0,
                LastTimeSeen: apiDriver.lastSeen ?? '',
                status: apiDriver.status ?? '',
                location: { latitude: apiDriver.latitude?? 0, longitude: apiDriver.longitude?? 0 },
            }
        )))
        );
        return drivers;
    }
}
