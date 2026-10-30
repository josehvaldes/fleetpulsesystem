declare module 'alerts_mfe/AlertsDashboard' {
  import { ComponentType } from 'react';

  interface MfeProps {
    getAuthToken: () =>Promise<string | null> ;
    apiBaseUrl?: string;
  }

  const Alerts: ComponentType<MfeProps>;
  export default Alerts;
}