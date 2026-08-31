
export type RiskLevel = "Low" | "Medium" | "High";
export type AlertStatus = "New" | "InProgress" | "Resolved" | "Closed" | "OnError" | "Dismissed";

export interface Alert {
    id: string;
    driverId: string;
    eventLocation: {
        latitude: number;
        longitude: number;
    };
    exitSpeed: number;
    exitHeading: number;
    exitTime: string;
    zoneName: string;
    zoneType: string;
    riskLevel: RiskLevel;
    status: AlertStatus;
    assessment: string;
    recommendation: string;

    raisedAt: string; // ISO-8601
}