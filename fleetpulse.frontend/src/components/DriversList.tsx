import { useGpsPings } from "../hooks/useGpsPings";

export function DriversList() {
  const { drivers } = useGpsPings();

  return(
    <>
        <h2>Drivers</h2>
        <ul>
        {Object.keys(drivers).length === 0 ? (
          <p>No drivers found.</p>
        ) : (
            Object.values(drivers).map((ping) => (
              <li key={ping.driver_id} className="text-xs">
                <strong>{ping.driver_id}</strong>
                (<span style={{color: ping.status=='stopped'?'orange':'blue'}} >{ping.status}</span>)                 
              </li>
            ))
        )}
        </ul>
    </>
  );
}

