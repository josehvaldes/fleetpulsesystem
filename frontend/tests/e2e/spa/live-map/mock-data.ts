import type {
  GpsPingResponse,
  AlertResponse,
} from '../../../../fleetpulse.frontend/src/generated/signalrhub';

export const ping = {
  event_id: '123',
  driver_id: 'driver_123',
  latitude: -17.3785,
  longitude: -66.15203,
  speed: 15,
  heading: 0,
  status: 'moving',
  accuracy: 1,
  timestamp: '2026-11-03T12:00:00Z',
} satisfies GpsPingResponse;

export const alert = { 
  id: "alert_02", 
  driverId: "driver_2", 
  eventLatitude: -17.3785, 
  eventLongitude: -66.15203, 
  exitSpeed: 85, 
  exitTime: "2026-06-01T12:00:00Z", 
  zoneName: "Industrial Zone I", 
  zoneType: "industrial", 
  riskLevel: "High", 
  status: "New", 
  assessment: "mock assessmemt",
  recommendation: "mock recommendation",
  autoEscalate: true,
  raisedAt: "2026-08-27T10:00:00Z"
} satisfies AlertResponse;

