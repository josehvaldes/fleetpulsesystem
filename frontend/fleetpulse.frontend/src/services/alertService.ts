import { fetchAlerts } from "@/api/alerts/alerts";
import type { AlertRequestParams } from "@/api/alerts/types";
import type { PageResponse } from "@/api/genericTypes";
import { parseRiskLevel, parseAlertStatus, type Alert } from "@/types/alert";



export async function getAlerts(  pageNumber: number, 
    pageSize: number, 
    riskLevel?: string, 
    status?: string, 
    fromDate?: string, 
    toDate?: string  ): Promise<PageResponse<Alert>> {
  try {
    const response = await fetchAlerts({pageNumber, pageSize, riskLevel, status, fromDate, toDate} as AlertRequestParams);
    const data = response.items;
    const alerts = data.map((dto) => 
    { 
        const alert: Alert = {
            id: dto.id ?? '',
            driverId: dto.driverId ?? '',
            eventLocation: {
                latitude: dto.eventLatitude ?? 0,
                longitude: dto.eventLongitude ?? 0,
            },
            exitSpeed: dto.exitSpeed ?? 0,
            exitTime: dto.exitTime ?? '',
            zoneName: dto.zoneName ?? '',
            zoneType: dto.zoneType ?? '',
            assessment: dto.assessment ?? '',
            recommendation: dto.recommendation ?? '',
            raisedAt: dto.raisedAt ?? '',
            status: parseAlertStatus(dto.status ?? ''),
            riskLevel: parseRiskLevel(dto.riskLevel ?? '')
            };
            return alert;
        }
    );

    return {
        items: alerts,
        totalCount: response.totalCount,
        totalPages: response.totalPages,
        hasNextPage: response.hasNextPage,
        hasPreviousPage: response.hasPreviousPage,
        pageNumber: response.pageNumber,
        pageSize: response.pageSize,
    };
    
  } catch (error) {
    console.error('Error fetching alerts:', error);
    throw new Error('Failed to fetch alerts');
  }
}