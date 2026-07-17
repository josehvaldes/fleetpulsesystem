import './App.css'
import { MessageLog } from "./components/MessageLog";
function App() {

  return (
    <>
      <div id="center" className="">
        <div>
          <h1>GPS Ping Monitor </h1>
        </div>
        <MessageLog />
      </div>
    </>
  )
}

export default App
