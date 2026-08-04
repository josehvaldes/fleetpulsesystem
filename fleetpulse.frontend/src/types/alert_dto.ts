
export interface LocationDto {
    latitude: number;
    longitude: number;
}

export interface AlertDto {
    id: string;
    driver_id: string;
    exit_location: LocationDto;
    exit_speed: number;
    exit_heading: number;
    exit_time: string; // ISO-8601
    zone_name: string;

    agent_risk_level: "Low" | "Medium" | "High";
    agent_assessment: string;
    agent_recommendation: string;

    created_at: string; // ISO-8601

}