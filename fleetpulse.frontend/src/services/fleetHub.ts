import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from "@microsoft/signalr";
import type { GpsPing } from "../types/gps";
import type { AlertDto } from "../types/alert_dto";

// In dev, the .NET hub usually runs on https://localhost:7001 (or http://5000).
// Adjust to whatever launchSettings.json / appsettings says.
// const HUB_URL = "https://localhost:7234/v1/fleetHub";
const HUB_URL = import.meta.env.VITE_FLEET_HUB_URL;

const RECEIVE_GPS_PING = "ReceiveGpsPing";
const RECEIVE_ALERT = "ReceiveAlert";

export type PingHandler = (ping: GpsPing) => void;

export type AlertHandler = (alert: AlertDto) => void;

export type FleetHubConnectionStatus =
  | "disconnected"
  | "connecting"
  | "reconnecting"
  | "connected";

export type ConnectionStateHandler = (status: FleetHubConnectionStatus) => void;

class FleetHubService {
  private connection: HubConnection;
  private pingHandlers = new Set<PingHandler>();
  private alertHandlers = new Set<AlertHandler>();
  private connectionStateHandlers = new Set<ConnectionStateHandler>();
  private startPromise: Promise<void> | null = null;
  private startRetryTimer: ReturnType<typeof setTimeout> | null = null;
  private startRetryDelayMs = 2000;
  private readonly maxStartRetryDelayMs = 30000;
  private startRetryAttempts = 0;

  constructor() {
    this.connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, {
        // SignalR needs credentials because the hub allows credentials in CORS.
        withCredentials: true,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // backoff
      .configureLogging(LogLevel.Warning)
      .build();

    // The single server→client callback. We fan out to local subscribers.
    this.connection.on(RECEIVE_GPS_PING, (ping: GpsPing) => {
      this.pingHandlers.forEach((h) => h(ping));
    });

    this.connection.on(RECEIVE_ALERT, (alert: AlertDto) => {
      this.alertHandlers.forEach((h) => h(alert));
    });

    this.connection.onreconnecting(() => {
      this.emitConnectionState();
      console.warn("[FleetHub] reconnecting");
    });
    this.connection.onreconnected(() => {
      this.resetStartRetryState();
      this.emitConnectionState();
      console.log("[FleetHub] reconnected");
    });
    this.connection.onclose((err) => {
      this.emitConnectionState();
      console.warn("[FleetHub] closed", err);
      this.scheduleStartRetry();
    });
  }

  async start() {
    if (this.connection.state === HubConnectionState.Connected) {
      return;
    }

    if (this.connection.state === HubConnectionState.Connecting) {
      return this.startPromise ?? Promise.resolve();
    }

    if (this.connection.state === HubConnectionState.Reconnecting) {
      return;
    }

    if (this.startPromise) {
      return this.startPromise;
    }

    this.clearStartRetryTimer();
    this.emitConnectionState();

    this.startPromise = this.connection
      .start()
      .then(() => {
        this.resetStartRetryState();
        this.emitConnectionState();
        console.log("[FleetHub] connected");
      })
      .catch((err) => {
        this.startRetryAttempts += 1;

        const shouldLog =
          this.startRetryAttempts <= 3 || this.startRetryAttempts % 5 === 0;
        if (shouldLog) {
          console.warn(
            `[FleetHub] start failed (attempt ${this.startRetryAttempts}). Retrying in ${this.startRetryDelayMs}ms.`,
            err
          );
        }

        this.scheduleStartRetry();
      })
      .finally(() => {
        this.startPromise = null;
      });

    return this.startPromise;
  }

  async stop() {
    this.clearStartRetryTimer();
    this.resetStartRetryState();
    await this.connection.stop();
    this.emitConnectionState();
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

  // Subscribe to alerts from the server. Returns an unsubscribe function.
  onAlerts(handler: AlertHandler): () => void {
    this.alertHandlers.add(handler);
    return () => this.alertHandlers.delete(handler);
  }

  onConnectionState(handler: ConnectionStateHandler): () => void {
    this.connectionStateHandlers.add(handler);
    handler(this.getConnectionStatus());
    return () => this.connectionStateHandlers.delete(handler);
  }

  getConnectionStatus(): FleetHubConnectionStatus {
    switch (this.connection.state) {
      case HubConnectionState.Connected:
        return "connected";
      case HubConnectionState.Connecting:
        return "connecting";
      case HubConnectionState.Reconnecting:
        return "reconnecting";
      case HubConnectionState.Disconnected:
      default:
        return "disconnected";
    }
  }

  isConnected(): boolean {
    return this.getConnectionStatus() === "connected";
  }

  private emitConnectionState() {
    const status = this.getConnectionStatus();
    this.connectionStateHandlers.forEach((handler) => handler(status));
  }

  private clearStartRetryTimer() {
    if (!this.startRetryTimer) {
      return;
    }

    clearTimeout(this.startRetryTimer);
    this.startRetryTimer = null;
  }

  private resetStartRetryState() {
    this.clearStartRetryTimer();
    this.startRetryDelayMs = 2000;
    this.startRetryAttempts = 0;
  }

  private scheduleStartRetry() {
    if (this.startRetryTimer) {
      return;
    }

    const delay = this.startRetryDelayMs;
    this.startRetryDelayMs = Math.min(
      this.startRetryDelayMs * 2,
      this.maxStartRetryDelayMs
    );

    this.startRetryTimer = setTimeout(() => {
      this.startRetryTimer = null;
      void this.start();
    }, delay);
  }
}

// Singleton — one WebSocket per browser tab is what we want.
export const fleetHub = new FleetHubService();