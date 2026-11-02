export {};

declare global {
  interface Window {
    __mockHub: {
      emit: (eventName: string, ...args: unknown[]) => void;
    };
  }
}
