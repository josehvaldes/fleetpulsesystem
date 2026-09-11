import type { PageResponse } from "../genericTypes";
import type { AlertRequestParams } from "./types";
import type { AlertWire } from "@/api/alerts/types";
import { sendRequest } from "@/api/genericRequest";

export async function fetchAlerts(
    requestParams: AlertRequestParams
): Promise<PageResponse<AlertWire>> {
  try {
    const queryParams = new URLSearchParams({
      pagenumber: requestParams.pageNumber.toString(),
      pagesize: requestParams.pageSize.toString(),
        ...(requestParams.riskLevel ? { riskLevel: requestParams.riskLevel } : {}),
        ...(requestParams.status ? { status: requestParams.status } : {}),
      ...(requestParams.from ? { fromDate: requestParams.from } : {}),
      ...(requestParams.to ? { toDate: requestParams.to } : {}),
    });
    console.log('Fetching alerts with query params:', queryParams.toString());
    const data = await sendRequest<PageResponse<AlertWire>>(`/alerts?${queryParams.toString()}`, {
      method: "GET",
    });
    return data;
  } catch (error) {
    console.error('Error fetching alerts:', error);
    throw new Error('Failed to fetch alerts');
  }
}