import type { User } from "./user";

export interface AuthSession {
  accessToken: string;
  user: User;
  expiresAt: number;
}