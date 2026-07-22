
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "https://localhost:7234/api";
export const API_VERSION = import.meta.env.VITE_API_VERSION || "v1";
export const API_KEY = import.meta.env.VITE_API_KEY || "your-api-key-here"; // Replace with your actual API key or use environment variables 
import { store } from "../store/store";


export interface ApiRequestOptions extends RequestInit {
  excludeApiVersion?: boolean;
}

export interface ErrorDetails {
  status: number;
  title: string;
  detail: string;
  instance: string;
}

function joinUrlSegments(...segments: string[]): string {
  return segments
    .map((segment) => segment.replace(/^\/+|\/+$/g, ""))
    .filter(Boolean)
    .join("/");
}


export async function sendRequest<T>(
  endpoint: string,
  options: ApiRequestOptions = {}
): Promise<T> {
  const { excludeApiVersion = false, headers, ...requestInit } = options;
  const normalizedEndpoint = endpoint.replace(/^\/+/, "");
  const url = excludeApiVersion
    ? `${API_BASE_URL}/${normalizedEndpoint}`
    : `${API_BASE_URL}/${joinUrlSegments(API_VERSION, normalizedEndpoint)}`;

  const mergedHeaders = new Headers(headers);
  mergedHeaders.set("x-api-key", API_KEY);

  const accessToken = store.getState().auth.accessToken;
  if (accessToken && !mergedHeaders.has("Authorization")) {
    mergedHeaders.set("Authorization", `Bearer ${accessToken}`);
  }

  const response = await fetch(url, {
    ...requestInit,
    headers: mergedHeaders,
  });

  if (!response.ok) {
    const bodyDetails = JSON.parse(await response.text()) as ErrorDetails
    if (bodyDetails && typeof bodyDetails === "object" && "detail" in bodyDetails) {
      throw new Error(`${bodyDetails.title}: ${bodyDetails.detail}`);
    }
    else{
      throw new Error(`${requestInit.method || "GET"} failed with status: ${response.status}`);
    }
  }

  const contentType = response.headers.get("content-type") || "";
  if (contentType.includes("application/json")) {
    return (await response.json()) as T;
  }

  return (await response.text()) as T;
}