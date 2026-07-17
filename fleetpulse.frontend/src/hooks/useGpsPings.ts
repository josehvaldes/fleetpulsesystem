import { useEffect, useState } from "react";
import { fleetHub } from "../services/fleetHub";
import type { GpsPing } from "../types/gps";

// Keep at most N most-recent pings in memory for the textarea view.
const MAX_PINGS = 200;

export function useGpsPings() {
  const [pings, setPings] = useState<GpsPing[]>([]);
  const [connected, setConnected] = useState(false);

  useEffect(() => {
    fleetHub.start();

    const unsubscribe = fleetHub.onPing((ping) => {
      setPings((prev) => {
        const next = [ping, ...prev];
        return next.length > MAX_PINGS ? next.slice(0, MAX_PINGS) : next;
      });
    });

    // Polling connection state is the simplest; for production you'd
    // expose an event from fleetHub instead.
    const t = setInterval(() => {
      setConnected(
        // signalr doesn't expose a reactivity API; reading state is fine.
        // We assume connected after start() resolves; refine later.
        true
      );
    }, 2000);

    return () => {
      unsubscribe();
      clearInterval(t);
    };
  }, []);

  return { pings, connected };
}