
import { login as loginRequest } from "../api/login";
import type { AuthSession } from "../types/auth";

export async function login(username: string, password: string): Promise<AuthSession> {
  const response = await loginRequest(username, password);

  if (!response?.accessToken || !response.username) {
    throw new Error("Login failed: Invalid response from server.");
  }

  const session: AuthSession = {
    accessToken: response.accessToken,
    user: {
      id: response.username,
      username: response.username,
    },
    expiresAt: Date.now() + response.expiresIn * 1000,
  };

  return session;
}

