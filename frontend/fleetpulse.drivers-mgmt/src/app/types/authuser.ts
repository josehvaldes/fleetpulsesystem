export interface AuthUser {
  token: string;
  userId: string;
  username: string;
  expiresAt?: string; // ISO 8601 formatted date-time string indicating when the token expires
}