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
                id: apiDriver.id,
                name: apiDriver.name,
                speed: apiDriver.speed,
                LastTimeSeen: apiDriver.LastTimeSeen,
                status: apiDriver.status,
                location: { latitude: apiDriver.latitude, longitude: apiDriver.longitude },
            }
        )))
        );
        console.log("Mapped drivers:", drivers.subscribe(data => console.log(data)));
        return drivers;
    }
}
