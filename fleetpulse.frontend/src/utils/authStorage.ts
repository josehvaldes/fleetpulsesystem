import type { User } from "../types/user";

export interface AuthSession {
  accessToken: string;
  user: User;
  expiresAt: number;
}

const AUTH_SESSION_STORAGE_KEY = "fleetpulse.auth.session";

function isValidSession(session: unknown): session is AuthSession {
  if (!session || typeof session !== "object") {
    return false;
  }

  const candidate = session as Partial<AuthSession>;

  return (
    typeof candidate.accessToken === "string" &&
    typeof candidate.expiresAt === "number" &&
    !!candidate.user &&
    typeof candidate.user.id === "string" &&
    typeof candidate.user.username === "string"
  );
}

export function getStoredAuthSession(): AuthSession | null {
  const rawSession = localStorage.getItem(AUTH_SESSION_STORAGE_KEY);
  if (!rawSession) {
    return null;
  }

  try {
    const parsedSession = JSON.parse(rawSession) as unknown;

    if (!isValidSession(parsedSession)) {
      clearStoredAuthSession();
      return null;
    }

    if (parsedSession.expiresAt <= Date.now()) {
      clearStoredAuthSession();
      return null;
    }

    return parsedSession;
  } catch {
    clearStoredAuthSession();
    return null;
  }
}

export function setStoredAuthSession(session: AuthSession): void {
  localStorage.setItem(AUTH_SESSION_STORAGE_KEY, JSON.stringify(session));
}

export function clearStoredAuthSession(): void {
  localStorage.removeItem(AUTH_SESSION_STORAGE_KEY);
}

export function getStoredAccessToken(): string | null {
  return getStoredAuthSession()?.accessToken ?? null;
}