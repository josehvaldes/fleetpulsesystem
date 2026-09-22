
import {Component, inject} from '@angular/core';
import { DriversApiService } from '@app/core/api/drivers-api.service';

@Component({
  selector: 'app-drivers-dashboard',
  templateUrl: './driversDashboard.html',
})
export class DriversDashboard {
    driverService = inject(DriversApiService);

    constructor() {

    }
}