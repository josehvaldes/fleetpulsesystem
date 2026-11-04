import './App.css'
import { Routes, Route } from 'react-router-dom';
import { FleetMap } from '@/features/fleet-map/';
import { AlertsDashboard } from '@/features/alerts/';
import { DriversDashboard } from '@/features/drivers/';
import { AuthGate } from './features/login/components/AuthGate';

function App() {
  return (
    <AuthGate>
      <Routes>
        <Route path="/" element={<FleetMap />} />
        <Route path="/alerts" element={<AlertsDashboard />} />
        <Route path="/drivers" element={<DriversDashboard />} />
      </Routes>
    </AuthGate>
  )
}

export default App
