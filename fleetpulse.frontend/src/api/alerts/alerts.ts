import type { PageResponse } from "../genericTypes";
import type { AlertRequestParams, AlertResponse } from "./types";
import { sendRequest } from "@/api/genericRequest";

export async function fetchAlerts(
    requestParams: AlertRequestParams
): Promise<PageResponse<AlertResponse>> {
  try {
    const queryParams = new URLSearchParams({
      pagenumber: requestParams.pageNumber.toString(),
      pagesize: requestParams.pageSize.toString(),
        ...(requestParams.riskLevel ? { riskLevel: requestParams.riskLevel } : {}),
        ...(requestParams.status ? { status: requestParams.status } : {}),
      ...(requestParams.fromDate ? { fromDate: requestParams.fromDate } : {}),
      ...(requestParams.toDate ? { toDate: requestParams.toDate } : {}),
    });
    const data = await sendRequest<PageResponse<AlertResponse>>(`/alerts?${queryParams.toString()}`, {
      method: "GET",
    });
    return data;
  } catch (error) {
    console.error('Error fetching alerts:', error);
    throw new Error('Failed to fetch alerts');
  }
}