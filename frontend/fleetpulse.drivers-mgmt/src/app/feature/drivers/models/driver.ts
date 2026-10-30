import type { GpsLocation } from './gpsLocation';

export interface Driver {
    id: string;
    name: string;
    speed: number;
    LastTimeSeen: string;
    status: string;
    location: GpsLocation;
}