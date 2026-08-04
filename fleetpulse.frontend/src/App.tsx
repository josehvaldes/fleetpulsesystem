import './App.css'
import { Login } from "./components/Login";
import { FleetMap } from "./components/FleetMap";
import { MessageLog } from "./components/MessageLog";
import { DriversList } from "./components/DriversList";
import { useAuth } from "./hooks/useAuth";
import { AlertsBox } from './components/AlertsBox';
function App() {
  const { isAuthenticated, logout } = useAuth();

  if (!isAuthenticated) {
    return <Login />;
  }

  return (
    <>
      <div id="center">
        <div className='border border-red-500 w-full grid grid-cols-[4rem_auto_4rem] gap-4 p-2 justify-stretch items-center'>
          <div className='text-sm'>
            User Info
          </div>
          <div>
            <h2>GPS Ping Monitor </h2>
          </div>
          <div>
            <button className='bg-blue-500 text-white rounded px-2 py-2 hover:bg-blue-600 text-sm'
            onClick={() => {
              logout();
            }}> Logout
          </button>
          </div>
        </div>
        <div className='grid grid-cols-7 gap-1 border border-blue-500 w-full'>
          <div className='col-span-1 border border-green-500'>
            
            <DriversList />
          </div>
          <div className='col-span-5' >
            <FleetMap />            
          </div>
          <div className='col-span-1'><AlertsBox /></div>
        </div>
        <div className='w-full border border-red-500'>
          <MessageLog />
        </div>
      </div>
    </>
  )
}

export default App
