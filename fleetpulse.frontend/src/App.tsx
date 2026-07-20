import './App.css'
import { FleetMap } from "./components/FleetMap";
import { MessageLog } from "./components/MessageLog";
function App() {

  return (
    <>
      <div id="center" className="">
        <div className='border border-red-500 w-full'>
          <h1>GPS Ping Monitor </h1>
        </div>
        <div className='grid grid-cols-7 gap-4 border border-blue-500 w-full'>
          <div className='col-span-1 border border-green-500'>
            Navigation bar
          </div>
          <div className='col-span-4' ><FleetMap /></div>
          <div className='col-span-2'><MessageLog /></div>
        </div>
        
      </div>
    </>
  )
}

export default App
