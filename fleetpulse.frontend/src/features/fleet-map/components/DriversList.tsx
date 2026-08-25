import { useGpsPings } from "@/features/fleet-map/hooks/useGpsPings";

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
              <li key={ping.driverId} className="text-xs">
                <strong>{ping.driverId}</strong>
                (<span style={{color: ping.status=='stopped'?'orange':'blue'}} >{ping.status}</span>)                 
              </li>
            ))
        )}
        </ul>
    </>
  );
}

