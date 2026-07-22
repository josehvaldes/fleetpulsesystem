

import { sendRequest } from "./genericRequest";
import type { LoginResponse } from "./responses";

export async function login(username: string, password: string): Promise<LoginResponse> {
  const response = await sendRequest<LoginResponse>("auth/login", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ username, password }),
  });
  return response;
}