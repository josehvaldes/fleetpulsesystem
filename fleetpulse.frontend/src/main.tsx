import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from "react-redux";
import { PersistGate } from "redux-persist/integration/react";
import { store, persistor} from "@/store/store";
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'


import './index.css'
import App from './App.tsx'

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

async function enableMocking() {
  if (isMockingEnabled) {
    const { worker } = await import('./mocks/browser');

    await worker.start({ onUnhandledRequest: 'warn' });
  }
}

enableMocking().then(() => {
  const app = (
    <BrowserRouter >
      <App />
    </BrowserRouter>
  );

  createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Provider store={store}>
      <PersistGate loading={null} persistor={persistor}>
        <QueryClientProvider client={queryClient}>
          {app}
        </QueryClientProvider>
      </PersistGate>
    </Provider>
  </StrictMode>,
)
});



