import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from "react-redux";
import { PersistGate } from "redux-persist/integration/react";
import { store, persistor} from "@/store/store";
import { BrowserRouter } from 'react-router-dom';

import './index.css'
import App from './App.tsx'

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
        {app}
      </PersistGate>
    </Provider>
  </StrictMode>,
)
});



