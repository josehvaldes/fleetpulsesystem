import { Routes } from '@angular/router';
import { DriversDashboard } from '@app/feature/drivers/components/driversDashboard';
import { SettingsPage } from '@app/feature/settings/components/settingsPage';

export const routes: Routes = [
	{ path: 'drivers', component: DriversDashboard },
	{ path: 'settings', component: SettingsPage },
	{ path: '', pathMatch: 'full', redirectTo: 'drivers' },
];
