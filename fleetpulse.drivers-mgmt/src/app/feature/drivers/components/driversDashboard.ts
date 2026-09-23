
import { Component, inject, input, OnInit, signal } from '@angular/core';
import { Driver } from '../models/driver';
import { Drivers } from '../services/drivers';
import { HlmTableImports } from '@app/shared/ui/table/src';
import { CommonModule } from '@angular/common';
import { environment } from '@env/environment';
import { setAuthTokenGetter, setApiBaseUrlOverride } from "@app/core/api/authTokenProvider";

@Component({
  selector: 'app-drivers-dashboard',
    imports: [CommonModule, HlmTableImports],
  templateUrl: './driversDashboard.html',
})
export class DriversDashboardComponent implements OnInit {

    /// Inputs for API base URL and authentication token retrieval function
    public readonly apiBaseUrl = input(environment.apiUrl);
    public readonly getAuthToken = input<() => Promise<string|null>>(async () => '');

    /// Service for fetching driver data
    private readonly driverService = inject(Drivers);
    /// Signal holding the list of drivers
    public readonly drivers = signal<Driver[]>([]);

    public ngOnInit(): void {
        setApiBaseUrlOverride(this.apiBaseUrl());
        setAuthTokenGetter(this.getAuthToken());
        
        this.driverService.getDrivers().subscribe((drivers: Driver[]) => {
            this.drivers.set(drivers);
        });
    }
}