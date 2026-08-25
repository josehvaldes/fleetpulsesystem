

import { sendRequest } from "@/api/genericRequest";
import type { LoginResponse } from "@/api/auth/types";

export async function login(username: string, password: string): Promise<LoginResponse> {
  
  const response = await sendRequest<LoginResponse>("/login", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ username, password }),
  });
  return response;
}