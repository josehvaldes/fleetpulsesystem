export interface AuthTokenProvider {
  (): Promise<string | null>;
}

let authTokenProvider: AuthTokenProvider | null = null;
let readyResolvers: (() => void)[] = [];

export function setAuthTokenProvider(provider: AuthTokenProvider | null): void {
  authTokenProvider = provider;

  if (provider !== null) {
    readyResolvers.forEach((resolve) => resolve());
    readyResolvers = [];
  }
}

export async function getAuthToken(): Promise<string | null> {
  const provider = authTokenProvider;

  return provider === null ? null : await provider();
}

// Resolves once a provider is registered, so callers can avoid starting
// authenticated connections before the auth gate has finished its setup effect.
export function waitForAuthTokenProvider(): Promise<void> {
  if (authTokenProvider !== null) {
    return Promise.resolve();
  }

  return new Promise<void>((resolve) => readyResolvers.push(resolve));
}
