import { MessageLog } from "@/features/fleet-map/components/MessageLog";
import { DriversList } from "@/features/fleet-map/components/DriversList";
import { AlertsBox } from "@/features/fleet-map/components/AlertsBox";
import { MapView } from "@/features/fleet-map/components/MapView";
import { useAuth } from "@/features/login/hooks/useAuth";

export function FleetMap() {
  const { logout } = useAuth();
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
            <MapView />            
          </div>
          <div className='col-span-1'><AlertsBox /></div>
        </div>
        <div className='w-full border border-red-500'>
          <MessageLog />
        </div>
      </div>
    </>
  );
}