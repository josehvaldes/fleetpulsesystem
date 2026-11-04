

interface LoginResponse {
  accessToken: string;
  username: string;
  tokenType: string;
  expiresIn: number; // in seconds
}

export type { LoginResponse };