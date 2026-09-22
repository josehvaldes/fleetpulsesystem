import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { DriverStateResponse } from '@app/core/contracts/driverstateresponse';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class DriversApiService {
    private readonly http = inject(HttpClient);
    getDrivers(): Observable<DriverStateResponse[]>  {
        const driversDto = this.http.get<DriverStateResponse[]>('/api/v1/drivers');
        return driversDto;
    }
}