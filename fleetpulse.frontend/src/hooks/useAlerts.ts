import { useEffect, useState } from "react";
import { fleetHub } from "@/services/fleetHub";
import type { Alert } from "@/types/alert";
import type { AlertDto } from "@/types/alert_dto";

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
          driverId: newAlertDto.driverId,
          eventLocation: {
            latitude: newAlertDto.eventLatitude,
            longitude: newAlertDto.eventLongitude,
          },
          exitSpeed: newAlertDto.exitSpeed,
          exitHeading: newAlertDto.exitHeading,
          exitTime: new Date(newAlertDto.exitTime),
          zoneName: newAlertDto.zoneName,
          riskLevel: newAlertDto.riskLevel,
          assessment: newAlertDto.assessment,
          recommendation: newAlertDto.recommendation,
          createdAt: new Date(newAlertDto.raisedAt),
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
