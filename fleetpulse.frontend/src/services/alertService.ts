import { fetchAlerts } from "@/api/alerts/alerts";
import type { AlertRequestParams } from "@/api/alerts/types";
import type { Alert } from "@/types/alert";

interface AlertsPage {
    data: Alert[];
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
    pageNumber: number;
    pageSize: number;
}

export async function getAlerts(  pageNumber: number, pageSize: number, riskLevel?: string, status?: string, fromDate?: string, toDate?: string  ): Promise<AlertsPage> {
  try {
    const response = await fetchAlerts({pageNumber, pageSize, riskLevel, status, fromDate, toDate} as AlertRequestParams);
    const data = response.data;
    const alerts = data.map((dto) => 
        ({
            id: dto.id,
            driverId: dto.driverId,
            eventLocation: {
                latitude: dto.eventLatitude,
                longitude: dto.eventLongitude,
            },
            exitSpeed: dto.exitSpeed,
            exitHeading: dto.exitHeading,
            exitTime: dto.exitTime,
            zoneName: dto.zoneName,
            zoneType: dto.zoneType,
            riskLevel: dto.riskLevel,
            assessment: dto.assessment,
            status: dto.status,
            recommendation: dto.recommendation,
            raisedAt: dto.raisedAt,
        } as Alert)
    );

    return {
        data: alerts,
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