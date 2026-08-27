import { useEffect, useState } from "react";
import { fleetHub } from "@/services/fleetHub";
import type { Alert } from "@/types/alert";
import type { AlertDto } from "@/types/alert_dto";
import { addAlert  } from "@/store/alertSlice";
import type { AppDispatch } from "@/store/store";
import { useDispatch } from "react-redux";

const MAX_ALERTS = 500;
export function useAlerts() {
  const dispatch: AppDispatch = useDispatch();
  const [alerts, setAlerts] = useState<Alert[]>([]);

  useEffect(() => {
    fleetHub.start();

    const unsubscribe = fleetHub.onAlerts((newAlertDto: AlertDto) => {
      const newAlert: Alert = {
          id: newAlertDto.id,
          driverId: newAlertDto.driverId,
          eventLocation: {
            latitude: newAlertDto.eventLatitude,
            longitude: newAlertDto.eventLongitude,
          },
          exitSpeed: newAlertDto.exitSpeed,
          exitHeading: newAlertDto.exitHeading,
          exitTime: newAlertDto.exitTime,
          zoneName: newAlertDto.zoneName,
          riskLevel: newAlertDto.riskLevel,
          assessment: newAlertDto.assessment,
          recommendation: newAlertDto.recommendation,
          raisedAt: newAlertDto.raisedAt,
        }

      setAlerts((prev) => {
        const combined = [newAlert, ...prev];
        const alerts = combined.length > MAX_ALERTS ? combined.slice(0, MAX_ALERTS) : combined;
        return alerts;
      });

      dispatch(addAlert(newAlert));

    });
    return () => {
      unsubscribe();
    };
  }, []);

  return alerts;
}
