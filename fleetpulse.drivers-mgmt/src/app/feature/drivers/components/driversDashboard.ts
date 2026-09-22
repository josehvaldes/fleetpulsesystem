
import { Component, inject, signal } from '@angular/core';
import { Driver } from '../models/driver';
import { Drivers } from '../services/drivers';
import { HlmTableImports } from '@app/shared/ui/table/src';
import { CommonModule } from '@angular/common';
@Component({
    imports: [CommonModule, HlmTableImports],
  selector: 'app-drivers-dashboard',
  templateUrl: './driversDashboard.html',
})
export class DriversDashboard {
    driverService = inject(Drivers);
    drivers = signal<Driver[]>([]);

    constructor() {
        this.driverService.getDrivers().subscribe(drivers => {
            this.drivers.set(drivers);
        });
    }
}