
export type RiskLevel = "Low" | "Medium" | "High";


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

    riskLevel: RiskLevel;
    assessment: string;
    recommendation: string;

    raisedAt: string; // ISO-8601
}