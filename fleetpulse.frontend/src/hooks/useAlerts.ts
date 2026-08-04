import { useEffect, useState } from "react";
import { fleetHub } from "../services/fleetHub";
import type { Alert } from "../types/alert";
import type { AlertDto } from "../types/alert_dto";

const MAX_ALERTS = 500;
export function useAlerts() {
  const [alerts, setAlerts] = useState<Alert[]>([]);

  useEffect(() => {
    fleetHub.start();

    const unsubscribe = fleetHub.onAlerts((newAlertDto: AlertDto) => {
        console.log(" * Received alert:", newAlertDto);

      setAlerts((prev) => {
        
        const newAlert: Alert = {
          id: newAlertDto.id,
          driverId: newAlertDto.driver_id,
          exitLocation: {
            latitude: newAlertDto.exit_location.latitude,
            longitude: newAlertDto.exit_location.longitude,
          },
          exitSpeed: newAlertDto.exit_speed,
          exitHeading: newAlertDto.exit_heading,
          exitTime: new Date(newAlertDto.exit_time),
          zoneName: newAlertDto.zone_name,
          riskLevel: newAlertDto.agent_risk_level,
          assessment: newAlertDto.agent_assessment,
          recommendation: newAlertDto.agent_recommendation,
          createdAt: new Date(newAlertDto.created_at),
        }

        const combined = [newAlert, ...prev];

        const alerts = combined.length > MAX_ALERTS ? combined.slice(0, MAX_ALERTS) : combined;
        return alerts;
      });

    });
    return () => {
      unsubscribe();
    };
  }, []);

  return alerts;
}
