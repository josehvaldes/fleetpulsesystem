// Drop-in replacement for "@microsoft/signalr", aliased in vite.config.ts when VITE_ENABLE_MOCKS=true.
/* eslint-disable @typescript-eslint/no-explicit-any */
type Handler = (...args: any[]) => void;

export const HubConnectionState = {
  Disconnected: "Disconnected",
  Connecting: "Connecting",
  Connected: "Connected",
  Disconnecting: "Disconnecting",
  Reconnecting: "Reconnecting",
} as const;
type State = (typeof HubConnectionState)[keyof typeof HubConnectionState];

export class HubConnection {
  private handlers = new Map<string, Set<Handler>>();
  private reconnectingHandlers: Handler[] = [];
  private reconnectedHandlers: Handler[] = [];
  private closeHandlers: Handler[] = [];
  public state: State = HubConnectionState.Disconnected;
  public connectionId: string | null = null;
  public options: unknown;

  constructor(options?: unknown) {
    console.log("[MockHub] initialized with options:", options);
    this.options = options;
    // Available as soon as the service is created so tests don't depend on start().
    (globalThis as any).__mockHub = this;
  }

  on(event: string, handler: Handler) {
    if (!this.handlers.has(event)) this.handlers.set(event, new Set());
    this.handlers.get(event)!.add(handler);
  }
  off(event: string, handler?: Handler) {
    if (handler) this.handlers.get(event)?.delete(handler);
    else this.handlers.delete(event);
  }
  async start() {
    this.state = HubConnectionState.Connecting;
    await new Promise((r) => setTimeout(r, 0));
    this.state = HubConnectionState.Connected;
    this.connectionId = "mock-connection";
  }
  async stop() {
    this.state = HubConnectionState.Disconnected;
    this.connectionId = null;
  }
  async invoke(method: string, ..._args: unknown[]) {
    if (method === "SubscribeFleet") return true;
    return null;
  }
  async send(_method: string, ..._args: unknown[]) {}

  onreconnecting(handler: Handler) { this.reconnectingHandlers.push(handler); }
  onreconnected(handler: Handler) { this.reconnectedHandlers.push(handler); }
  onclose(handler: Handler) { this.closeHandlers.push(handler); }

  // --- Test-only API ---
  emit(event: string, ...args: unknown[]) {
    this.handlers.get(event)?.forEach((h) => h(...args));
  }
  simulateReconnecting(err?: Error) {
    this.state = HubConnectionState.Reconnecting;
    this.reconnectingHandlers.forEach((h) => h(err));
  }
  simulateReconnected() {
    this.state = HubConnectionState.Connected;
    this.reconnectedHandlers.forEach((h) => h(this.connectionId));
  }
  simulateClose(err?: Error) {
    this.state = HubConnectionState.Disconnected;
    this.closeHandlers.forEach((h) => h(err));
  }
}

export class HubConnectionBuilder {
  private options: unknown;
  withUrl(_url: string, options?: unknown) { this.options = options; return this; }
  withAutomaticReconnect(_delays?: number[]) { return this; }
  withHubProtocol(_p?: unknown) { return this; }
  configureLogging(_l?: unknown) { return this; }
  build() { return new HubConnection(this.options); }
}

export const LogLevel = { Trace: 0, Debug: 1, Information: 2, Warning: 3, Error: 4, Critical: 5, None: 6 };
export const HttpTransportType = { WebSockets: 1, ServerSentEvents: 2, LongPolling: 4 };
export const TransferFormat = { Text: 1, Binary: 2 };
