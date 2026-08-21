
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
    exitTime: Date;
    zoneName: string;

    riskLevel: RiskLevel;
    assessment: string;
    recommendation: string;

    createdAt: Date;
}