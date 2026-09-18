import type { User } from "@/types/user";

export interface AuthSession {
  accessToken: string;
  user: User;
  expiresAt: number;
}