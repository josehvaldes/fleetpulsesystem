import React, { Suspense } from 'react';

import { store } from "@/store/store";

// Import the remote component
const AlertsMfe = React.lazy(() => import('alerts_mfe/AlertsDashboard'));

export default function AlertsMfeWrapper() {
  // Assuming you store the token in Redux
  const token = store.getState().auth.accessToken;

  // Expose the getToken method as described in frontend.md
  const getAuthToken = React.useCallback(() => {
    return token;
  }, [token]);

  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || "https://localhost:7234/api";

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