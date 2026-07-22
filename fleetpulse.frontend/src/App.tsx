import './App.css'
import { Login } from "./components/Login";
import { FleetMap } from "./components/FleetMap";
import { MessageLog } from "./components/MessageLog";
import { DriversList } from "./components/DriversList";
import { useAuth } from "./hooks/useAuth";

function App() {
  const { isAuthenticated, isHydrated, logout } = useAuth();

  if (!isHydrated) {
    return null;
  }

  if (!isAuthenticated) {
    return <Login />;
  }



  return (
    <>
      <div id="center" className="">
        <div className='border border-red-500 w-full'>
          <button className='bg-blue-500 text-white rounded px-4 py-2 hover:bg-blue-600'
            onClick={() => {
              logout();
            }}> Logout</button>

          <h1>GPS Ping Monitor </h1>
        </div>
        <div className='grid grid-cols-7 gap-4 border border-blue-500 w-full'>
          <div className='col-span-1 border border-green-500'>
            <DriversList />
          </div>
          <div className='col-span-4' ><FleetMap /></div>
          <div className='col-span-2'><MessageLog /></div>
        </div>
      </div>
    </>
  )
}

export default App
