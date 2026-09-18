declare module 'alerts_mfe/AlertsDashboard' {
  import { ComponentType } from 'react';

  interface AlertsMfeProps {
    getAuthToken: () => string | null;
    apiBaseUrl?: string | null;
  }

  const Alerts: ComponentType<AlertsMfeProps>;
  export default Alerts;
}