// Holds a live reference to the host's token getter so API calls always read a fresh token.
type AuthTokenGetter = () => string | null;

let authTokenGetter: AuthTokenGetter | null = null;

export function setAuthTokenGetter(getter: AuthTokenGetter | null): void {
  authTokenGetter = getter;
}

export function getCurrentAuthToken(): string | null {
  return authTokenGetter ? authTokenGetter() : null;
}

// Host-provided override for the API base URL, so it always matches the API that issued the token.
let apiBaseUrlOverride: string | null = null;

export function setApiBaseUrlOverride(baseUrl: string | null | undefined): void {
  apiBaseUrlOverride = baseUrl || null;
}

export function getApiBaseUrlOverride(): string | null {
  return apiBaseUrlOverride;
}
