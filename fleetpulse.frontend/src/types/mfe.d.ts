declare module 'alerts_mfe/AlertsDashboard' {
  import { ComponentType } from 'react';

  interface MfeProps {
    getAuthToken: () => string | null;
    apiBaseUrl?: string | null;
  }

  const Alerts: ComponentType<MfeProps>;
  export default Alerts;
}