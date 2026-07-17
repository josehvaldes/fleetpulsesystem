export interface GpsPing {
  driver_id:        string;
  latitude:         number;
  longitude:        number;
  speed_kmh:        number;
  heading_degrees:  number;
  accuracy_meters:  number;
  status:           string;
  vehicle_type:     string | null;
  timestamp:        string; // ISO-8601
}