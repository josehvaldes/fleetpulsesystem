import type { RootState } from "@/store/store";
import { useSelector } from "react-redux";
import { useAlerts } from "../hooks/useAlerts";
import { AlertPopup } from "./AlertPopup";


export function AlertsBox()
{
    useAlerts();
    const alerts = useSelector((state: RootState) => state.alert.alerts);
    return(
        <div className="alerts-box border border-yellow-500 p-2">
            <h3 className="font-bold">Live Alerts</h3>
            {alerts.length === 0 ? (
                <p>No alerts received.</p>
            ) : (
                <ul className="flex flex-col justify-start text-left">
                    {alerts.map((alert) => (
                        (() => {
                            
                            return (
                                <li key={alert.id} className="text-xs flex flex-row justify-start">
                                    <AlertPopup alert={alert} />
                                </li>
                            );
                        })()
                    ))}
                </ul>
            )}
        </div>
    );
}