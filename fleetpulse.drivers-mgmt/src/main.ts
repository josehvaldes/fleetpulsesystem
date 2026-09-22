import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { environment } from './environments/environment';

async function prepareApp() {
  if (typeof window !== 'undefined' && environment.enableMsw) {
    const { worker } = await import('./mocks/browser');
    await worker.start();
  }
}
prepareApp().then(() => {
  bootstrapApplication(App, appConfig)
    .catch((err) => console.error(err));
});

