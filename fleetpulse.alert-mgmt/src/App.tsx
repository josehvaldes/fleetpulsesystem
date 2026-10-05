import './App.css'
import { AlertsDashboardContent } from '@/features/alerts/';
function App() {
  return (
    <>
      <AlertsDashboardContent  getAuthToken={async () => ""} />
    </>
  )
}

export default App
