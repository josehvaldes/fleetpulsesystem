import { store } from "@/store/store";
import { loadAppConfig } from "@/utils/appConfig";

const config = await loadAppConfig();
const API_BASE_URL = config.api.baseUrl || "http://localhost:8380/api";
const API_VERSION = config.api.version || "v1";
const API_KEY = import.meta.env.VITE_API_KEY || "your-api-key-here"; // Replace with your actual API key or use environment variables 

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