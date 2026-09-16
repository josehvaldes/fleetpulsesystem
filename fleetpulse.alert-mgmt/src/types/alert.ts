export const alertStatuses = [
  "New",
  "InProgress",
  "Resolved",
  "Closed",
  "OnError",
  "Dismissed",
] as const;

export type AlertStatus = (typeof alertStatuses)[number];

export const riskLevels = ["Low", "Medium", "High"] as const;

export type RiskLevel = (typeof riskLevels)[number];

export function parseAlertStatus(value: string): AlertStatus {
  if ((alertStatuses as readonly string[]).includes(value)) {
    return value as AlertStatus;
  }

  throw new Error(`Unsupported alert status: ${value}`);
}

export function parseRiskLevel(value: string): RiskLevel {
  if ((riskLevels as readonly string[]).includes(value)) {
    return value as RiskLevel;
  }

  throw new Error(`Unsupported risk level: ${value}`);
}

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