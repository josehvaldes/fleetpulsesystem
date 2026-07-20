import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from "@microsoft/signalr";
import type { GpsPing } from "../types/gps";

// In dev, the .NET hub usually runs on https://localhost:7001 (or http://5000).
// Adjust to whatever launchSettings.json / appsettings says.
// const HUB_URL = "https://localhost:7234/v1/fleetHub";
const HUB_URL = import.meta.env.VITE_FLEET_HUB_URL;

export type PingHandler = (ping: GpsPing) => void;

class FleetHubService {
  private connection: HubConnection;
  private pingHandlers = new Set<PingHandler>();

  constructor() {
    this.connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, {
        // SignalR needs credentials because the hub allows credentials in CORS.
        withCredentials: true,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // backoff
      .configureLogging(LogLevel.Information)
      .build();

    // The single server→client callback. We fan out to local subscribers.
    this.connection.on("ReceiveGpsPing", (ping: GpsPing) => {
      this.pingHandlers.forEach((h) => h(ping));
    });

    this.connection.onreconnecting(() =>
      console.log("[FleetHub] reconnecting…")
    );
    this.connection.onreconnected(() =>
      console.log("[FleetHub] reconnected")
    );
    this.connection.onclose((err) =>
      console.log("[FleetHub] closed", err)
    );
  }

  async start() {
    if (this.connection.state === HubConnectionState.Connected) return;
    try {
      await this.connection.start();
      console.log("[FleetHub] connected");
    } catch (err) {
      console.error("[FleetHub] start failed", err);
      // withAutomaticReconnect won't kick in if start() itself fails,
      // so retry once after a short delay.
      setTimeout(() => this.start(), 10000);
    }
  }

  async stop() {
    await this.connection.stop();
  }

  // Subscribe to pings from the server. Returns an unsubscribe function.
  onPing(handler: PingHandler): () => void {
    this.pingHandlers.add(handler);
    return () => this.pingHandlers.delete(handler);
  }

  // Optional: subscribe to a fleet group on the server
  async subscribeFleet(fleetId: string) {
    await this.connection.invoke("SubscribeFleet", fleetId);
  }
}

// Singleton — one WebSocket per browser tab is what we want.
export const fleetHub = new FleetHubService();