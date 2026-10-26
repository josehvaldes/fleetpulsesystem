export interface GpsPing {
  driver_id:        string;
  latitude:         number;
  longitude:        number;
  speed:        number;
  heading:  number;
  accuracy:  number;
  status:           string;
  vehicle:     string | null;
  timestamp:        string; // ISO-8601
}