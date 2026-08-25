import './App.css'
import { Login } from "@/features/login/";
import { useAuth } from "@/features/login/";
import { FleetMap } from '@/features/fleet-map/';

function App() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Login />;
  }

  return (
    <FleetMap />
  )
}

export default App
