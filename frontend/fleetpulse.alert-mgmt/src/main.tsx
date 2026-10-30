import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from 'react-redux'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { PersistGate } from "redux-persist/integration/react";
import './index.css'
import App from './App.tsx'
import { persistor, store } from './store/store.ts';


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


createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Provider store={store}>
      <PersistGate loading={null} persistor={persistor}>
        <QueryClientProvider client={queryClient}>
          <App />
        </QueryClientProvider>
      </PersistGate>
    </Provider>
  </StrictMode>,
)
