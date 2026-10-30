import { MessageLog } from "@/features/fleet-map/components/MessageLog";
import { DriversBox } from "@/features/fleet-map/components/DriversBox";
import { AlertsBox } from "@/features/fleet-map/components/AlertsBox";
import { MapView } from "@/features/fleet-map/components/MapView";
import { Header } from "@/components/layouts/header";

export function FleetMap() {

  return (
    <>
    <div>
        <Header />
        <div className='grid grid-cols-12 gap-1 border border-blue-500 w-full'>
          <div className='col-span-2 border border-green-500'>
            <DriversBox />
          </div>
          <div className='col-span-7' >
            <MapView />            
          </div>
          <div className='col-span-3'><AlertsBox /></div>
        </div>
        <div className='w-full border border-red-500'>
          <MessageLog />
        </div>
      </div>
    </>
  );
}