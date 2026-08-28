import './App.css'
import { Routes, Route } from 'react-router-dom';
import { Login } from "@/features/login/";
import { useAuth } from "@/features/login/";
import { FleetMap } from '@/features/fleet-map/';
import { AlertsDashboard } from '@/features/alerts/';
import { DriversDashboard } from '@/features/drivers/';

function App() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Login />;
  }

  return (
    <Routes>
      <Route path="/" element={<FleetMap />} />
      <Route path="/alerts" element={<AlertsDashboard />} />      
      <Route path="/drivers" element={<DriversDashboard />} />
    </Routes>
  )
}

export default App
