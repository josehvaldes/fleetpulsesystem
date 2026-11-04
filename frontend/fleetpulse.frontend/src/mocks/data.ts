import type { LastestDriverStateResponse } from "@/generated/api/types.gen";
import type { AlertResponse } from "@/generated/signalrhub";

const mockUserInfo = {
    username: 'admin',
    password: 'adminfleet',
    role: 'admin'
};

const mockAlerts:AlertResponse[] = [
    {
        id: "1",
        driverId: "driver1",
        eventLatitude: 37.7749,
        eventLongitude: -122.4194,
        exitSpeed: 65,
        exitHeading: 90,
        exitTime: "2023-10-01T12:00:00Z",
        zoneName: "Zone A",
        zoneType: "Type 1",
        riskLevel: "High",
        status: "New",
        assessment: "Assessment 1",
        recommendation: "Recommendation 1",
        raisedAt: "2026-08-30T12:05:00Z"
    } as AlertResponse,
    {
        id: "2",
        driverId: "driver2",
        eventLatitude: 34.0522,
        eventLongitude: -118.2437,
        exitSpeed: 55,
        exitHeading: 180,
        exitTime: "2023-10-02T14:30:00Z",
        zoneName: "Zone B",
        zoneType: "Type 2",
        status: "Closed",
        riskLevel: "Medium",
        assessment: "Assessment 2",
        recommendation: "Recommendation 2",
        raisedAt: "2026-08-31T14:35:00Z"
    } as AlertResponse,
    {
        id: "3",
        driverId: "driver3",
        eventLatitude: 33.0522,
        eventLongitude: -113.2437,
        exitSpeed: 55,
        exitHeading: 180,
        exitTime: "2023-10-02T14:30:00Z",
        zoneName: "Zone B",
        zoneType: "Type 2",
        status: "InProgress",
        riskLevel: "Medium",
        assessment: "Assessment 2",
        recommendation: "Recommendation 2",
        raisedAt: "2026-08-31T14:35:00Z"
    } as AlertResponse,
    {
        id: "4",
        driverId: "driver4",
        eventLatitude: 33.0522,
        eventLongitude: -113.2437,
        exitSpeed: 55,
        exitHeading: 180,
        exitTime: "2023-10-02T14:30:00Z",
        zoneName: "Zone B",
        zoneType: "Type 2",
        status: "Dismissed",
        riskLevel: "Medium",
        assessment: "Assessment 2",
        recommendation: "Recommendation 2",
        raisedAt: "2026-08-30T14:35:00Z"
    } as AlertResponse,
    {
        id: "5",
        driverId: "driver5",
        eventLatitude: 33.0522,
        eventLongitude: -113.2437,
        exitSpeed: 55,
        exitHeading: 180,
        exitTime: "2023-10-02T14:30:00Z",
        zoneName: "Zone B",
        zoneType: "Type 2",
        status: "OnError",
        riskLevel: "Medium",
        assessment: "Assessment 2",
        recommendation: "Recommendation 2",
        raisedAt: "2026-08-31T14:35:00Z"
    }as AlertResponse,
];



const mockDrivers: LastestDriverStateResponse[] = [
    {
        driverId: "test-01",
        speed: 50,
        lastSeen: "2026-09-21T00:00:00Z",
        status: "moving",
        latitude: -17.98650,
        longitude: -14.98754,
    },
    {
      driverId: "test-02",
      speed: 0,
      lastSeen: "2026-09-21T00:00:00Z",
      status: "stopped",
      latitude: -18.12345,
      longitude: -15.12345,
    },
    {
      driverId: "test-03",
      speed: 30,
      lastSeen: "2026-09-21T00:00:00Z",
      status: "moving",
      latitude: -19.54321,
      longitude: -16.54321,
    }
]
    
export { mockAlerts, mockUserInfo, mockDrivers };