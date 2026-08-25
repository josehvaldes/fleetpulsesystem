import { useEffect, useState } from "react";
import { fleetHub } from "@/services/fleetHub";
import type { GpsPing } from "@/types/gps";
import type { FleetHubConnectionStatus } from "@/services/fleetHub";

// Keep at most N most-recent pings in memory for the textarea view.
const MAX_PINGS = 200;

export type DriverLocations = Record<string, GpsPing>;

export function useGpsPings() {
  const [pings, setPings] = useState<GpsPing[]>([]);
  const [drivers, setDrivers] = useState<DriverLocations>({});
  const [status, setStatus] = useState<FleetHubConnectionStatus>(
    fleetHub.getConnectionStatus()
  );

  useEffect(() => {
    void fleetHub.start();

    const unsubscribePing = fleetHub.onPing((ping) => {
      console.log("Received ping:", ping);

      setDrivers((prev) => ({
        ...prev,
        [ping.driverId]: ping,
      }));
      setPings((prev) => {
        const next = [ping, ...prev];
        return next.length > MAX_PINGS ? next.slice(0, MAX_PINGS) : next;
      });
    });

    const unsubscribeConnection = fleetHub.onConnectionState(setStatus);

    return () => {
      unsubscribePing();
      unsubscribeConnection();
    };
  }, []);

  const connected = status === "connected";

  return { drivers, pings, status, connected };
}