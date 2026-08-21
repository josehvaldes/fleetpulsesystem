

export interface AlertDto {
    id: string;
    driverId: string;
    eventLatitude: number;
    eventLongitude: number;
    exitSpeed: number;
    exitHeading: number;
    exitTime: string; // ISO-8601
    zoneName: string;
    zoneType: string;

    riskLevel: "Low" | "Medium" | "High";
    assessment: string;
    recommendation: string;

    status: "New"| "InProgress" | "Resolved"| "Closed"| "OnError";
    raisedAt: string; // ISO-8601
}