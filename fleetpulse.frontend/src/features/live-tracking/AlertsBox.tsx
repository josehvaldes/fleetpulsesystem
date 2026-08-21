import { useAlerts } from "@/hooks/useAlerts";


export function AlertsBox()
{
    const alerts = useAlerts();
    function getColorForRiskLevel(riskLevel: string): string {
        switch (riskLevel.toLowerCase()) {
            case "low":
                return "text-green-500";
            case "medium":
                return "text-yellow-500";
            case "high":
                return "text-red-500";
            default:
                return "text-gray-500";
        }
    }
    return(
        <div className="alerts-box border border-yellow-500 p-2">
            <h2>Alerts</h2>
            {alerts.length === 0 ? (
                <p>No alerts received.</p>
            ) : (
                <ul>
                    {alerts.map((alert) => (
                        <li key={alert.id} className="text-xs">
                            <a href="#" className="underline hover:text-blue-500">
                            <strong
                            className={getColorForRiskLevel(alert.riskLevel)}
                            >{alert.riskLevel}</strong> - {alert.driverId}
                            </a>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}