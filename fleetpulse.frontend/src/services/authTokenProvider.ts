export interface AuthTokenProvider {
  (): Promise<string | null>;
}

let authTokenProvider: AuthTokenProvider | null = null;

export function setAuthTokenProvider(provider: AuthTokenProvider | null): void {
  authTokenProvider = provider;
}

export async function getAuthToken(): Promise<string | null> {
  const provider = authTokenProvider;

  return provider === null ? null : await provider();
}
