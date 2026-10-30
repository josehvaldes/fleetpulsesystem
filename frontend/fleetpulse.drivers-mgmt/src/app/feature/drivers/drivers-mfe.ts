import { createComponent } from '@angular/core';
import { createApplication } from '@angular/platform-browser';
import { appConfig } from '@app/app.config';
import { DriversDashboardComponent } from './components/driversDashboard';

export interface DriversMfeProps {
  apiBaseUrl: string;
  getAuthToken: () => Promise<string>;
}

export async function mountDriversDashboard(
  hostElement: Element,
  props: DriversMfeProps,
): Promise<() => void> {
  const appRef = await createApplication(appConfig);
  const componentRef = createComponent(DriversDashboardComponent, {
    environmentInjector: appRef.injector,
  });

  componentRef.setInput('apiBaseUrl', props.apiBaseUrl);
  componentRef.setInput('getAuthToken', props.getAuthToken);
  appRef.attachView(componentRef.hostView);
  hostElement.replaceChildren(componentRef.location.nativeElement);
  componentRef.changeDetectorRef.detectChanges();

  return () => {
    appRef.detachView(componentRef.hostView);
    componentRef.destroy();
    appRef.destroy();
  };
}