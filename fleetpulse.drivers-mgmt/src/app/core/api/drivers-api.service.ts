import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { DriverStateResponse } from '@app/core/contracts/driverstateresponse';
import { Observable, map } from 'rxjs';
import { Driver } from '@app/feature/drivers/models/driver';

@Injectable({
  providedIn: 'root'
})
export class DriversApiService {
    private readonly http = inject(HttpClient);
    getDrivers(): Observable<Driver[]>  {
        const driverDto = this.http.get<DriverStateResponse[]>('/api/v1/drivers');
        const drivers =  driverDto.pipe(
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
        return drivers;
    }
}