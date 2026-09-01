
interface AlertRequestParams {
    pageNumber: number;
    pageSize: number;
    riskLevel?: string;
    status?: string;
    fromDate?: string;
    toDate?: string;
}

interface AlertWire{
    id: string;
    driverId: string;
    eventLatitude: number;
    eventLongitude: number;
    exitSpeed: number;
    exitHeading: number;
    exitTime: string;
    zoneName: string;
    zoneType: string;
    assessment: string;
    recommendation: string;
    raisedAt: string; // ISO-8601

    riskLevel: string;
    status: string;
}

export type {AlertWire, AlertRequestParams };