import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { DriverStateResponse } from '@app/core/contracts/driverstateresponse';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})
export class DriversApiService {
    private readonly http = inject(HttpClient);
    private baseUrl = `${environment.apiUrl}/api/v1`;
    getDrivers(): Observable<DriverStateResponse[]>  {
        const driversDto = this.http.get<DriverStateResponse[]>(`${this.baseUrl}/drivers`);
        return driversDto;
    }
}