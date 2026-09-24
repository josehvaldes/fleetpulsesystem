import React, { Suspense } from 'react';
import { store } from "@/store/store";
import { loadRemote } from '@module-federation/enhanced/runtime';
import { loadAppConfig } from "@/utils/appConfig";
const appConfig = await loadAppConfig();

// Import the remote component
//const AlertsMfe = React.lazy(() => import('alerts_mfe/AlertsDashboard'));

const AlertsMfe = React.lazy(async () => {
  const module = await loadRemote<typeof import('alerts_mfe/AlertsDashboard')>(
    'alerts_mfe/AlertsDashboard',
  );

  if (!module) {
    throw new Error('Unable to load the alerts remote module.');
  }

  return {
    default: module.default,
  };
}
);

export default function AlertsMfeWrapper() {
  // Assuming you store the token in Redux
  const token = store.getState().auth.accessToken;

  // Expose the getToken method as described in frontend.md
  const getAuthToken = React.useCallback(() => {
    return token;
  }, [token]);

  const apiBaseUrl = appConfig.api.baseUrl || "https://localhost:7234/api";

  return (
    <div className="mfe-wrapper">
      <Suspense fallback={<div>Loading Alerts Module...</div>}>
        <AlertsMfe 
          getAuthToken={getAuthToken} 
          apiBaseUrl={apiBaseUrl} 
        />
      </Suspense>
    </div>
  );
}