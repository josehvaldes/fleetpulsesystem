import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { DriverStateResponse } from '@app/core/contracts/driverstateresponse';
import { defer, from, Observable, switchMap } from 'rxjs';
import { getCurrentAuthToken, getApiBaseUrlOverride } from "@app/core/api/authTokenProvider";

@Injectable({
  providedIn: 'root'
})
export class DriversApiService {
    private readonly http = inject(HttpClient);
  getDrivers(): Observable<DriverStateResponse[]>  {
    
    const apiBaseUrl = getApiBaseUrlOverride() || '';

    const baseUrl = apiBaseUrl.replace(/\/$/, '');
    console.log(`Fetching drivers from: [${baseUrl}/v1/drivers]`);

    return defer(() => from(getCurrentAuthToken())).pipe(
      switchMap(token => this.http.get<DriverStateResponse[]>(`${baseUrl}/v1/drivers`, {
        headers: new HttpHeaders({ Authorization: `Bearer ${token ?? ''}` }),
      })),
    );
    }
}