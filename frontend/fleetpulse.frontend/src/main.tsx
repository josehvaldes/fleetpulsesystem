import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from "react-redux";
import { PersistGate } from "redux-persist/integration/react";
import { store, persistor} from "@/store/store";
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'


import './index.css'
import App from './App.tsx'
import { initializeNativeFederation } from '@/federation/native-federation';
import { InitializeReactFederation } from './federation/react-federation.ts';
import { MsalProvider } from '@azure/msal-react';
import { PublicClientApplication } from '@azure/msal-browser';
import { msalConfig } from './utils/msalConfig.ts';


const queryClient = new QueryClient(
  {
    defaultOptions: {
      queries: {
        staleTime: 5 * 60 * 1000, // 5 minutes
        retry: 1, // Retry failed requests once
        refetchOnWindowFocus: false, // Disable refetching on window focus
        gcTime: 10 * 60 * 1000, // 10 minutes before garbage collecting unused query data
      },
    },
  }
);

const isMockingEnabled = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCKS === 'true';
const appWithSharedProviders = (
<Provider store={store}>
  <PersistGate loading={null} persistor={persistor}>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter >
        <App />
      </BrowserRouter>
    </QueryClientProvider>
  </PersistGate>
</Provider>);

const authMode = import.meta.env.VITE_AUTH_MODE;

if (authMode !== "local" && authMode !== "msal") {
  throw new Error(
    `Invalid VITE_AUTH_MODE "${authMode}". Expected "local" or "msal".`,
  );
}

async function createAppToRender() {
  if (authMode === "local") {
    return appWithSharedProviders;
  }

  const msalInstance = new PublicClientApplication(msalConfig);
  await msalInstance.initialize();
  console.log("MSAL instance initialized:");
  return (
    <MsalProvider instance={msalInstance}>
      {appWithSharedProviders}
    </MsalProvider>
  );
}

async function enableMocking() {
  if (isMockingEnabled) {
    const { worker } = await import('./mocks/browser');

    await worker.start({ onUnhandledRequest: 'warn' });
  }
}
enableMocking().then(async () => {
  console.log('Mocking enabled:', isMockingEnabled);

  await Promise.all([
    InitializeReactFederation(),
    initializeNativeFederation(),
  ]);

    console.log('Federation initialized successfully');
    const appToRender = await createAppToRender();
    
    createRoot(document.getElementById('root')!).render(
    <StrictMode>
      {appToRender}
    </StrictMode>,
    );
});








