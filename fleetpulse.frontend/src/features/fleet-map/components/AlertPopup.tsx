import { Button } from "@/components/ui/button";
import { Dialog, 
    DialogContent, 
    DialogDescription, 
    DialogHeader, 
    DialogTitle, 
    DialogTrigger } from "@/components/ui/dialog";
import type { AppDispatch } from "@/store/store";
import { removeAlert } from "@/store/alertSlice";
import type { Alert } from "@/types/alert";
import { riskLevelStyle } from "@/utils/fleethub_utils";
import { useState } from "react";
import { useDispatch } from "react-redux";
import { useAlertActions} from "@/features/alerts/";

type AlertPopupProps = {
    alert: Alert;
};

export function AlertPopup({ alert }: AlertPopupProps) {
    const { mutate: handleAlertAction, isPending: isSubmittingAction} = useAlertActions();
    const [open, setOpen] = useState(false)
    const dispatch: AppDispatch = useDispatch<AppDispatch>();
        
    const colorClass = riskLevelStyle(alert.riskLevel);
    const timeDiff = new Date().getTime() - new Date(alert.raisedAt).getTime();
    const minutesAgo = Math.floor(timeDiff / 60000);
    const timelabel = minutesAgo < 60 ? `${minutesAgo} minutes ago` : 
                        `${Math.floor(minutesAgo / 60)} hours ago`;

    const handleAction = (action:string) => {
        console.log(`${action} alert ${alert.id}`);

        handleAlertAction(
            { alertId: alert.id, action: action,

            },{
                onSuccess: () => {
                    console.log(`Successfully performed action ${action} on alert ${alert.id}`);
                    dispatch(removeAlert(alert.id));
                    setOpen(false);
                },
                onError: (error) => {
                    console.error(`Error performing action ${action} on alert ${alert.id}:`, error);
                }
            }
        );

        
    }

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger>
                <a className="text-xs hover:underline cursor-pointer">
                <strong
                        className={colorClass.color}
                        >{colorClass.label}</strong> | {alert.zoneName} | 
                        {alert.driverId} | 
                        { timelabel }
                </a>
            </DialogTrigger>
            <DialogContent>
                <DialogHeader>
                <DialogTitle>Alert Details: {colorClass.label}</DialogTitle>
                <DialogDescription>
                    Detailed information about the triggered alert.
                </DialogDescription>
                </DialogHeader>

                <div className="space-y-2">
                    <p><strong>Risk Level:</strong> <span className={colorClass.color}>{colorClass.label}</span></p>
                    <p><strong>Zone Name:</strong> {alert.zoneName}</p>
                    <p><strong>Driver ID:</strong> {alert.driverId}</p>
                    <p><strong>Exit Speed:</strong> {alert.exitSpeed} km/h</p>
                    <p><strong>Exit Heading:</strong> {alert.exitHeading}°</p>
                    <p><strong>Exit Time:</strong> {new Date(alert.exitTime).toLocaleString()}</p>
                    <p><strong>Raised At:</strong> {new Date(alert.raisedAt).toLocaleString()} ({timelabel})</p>
                    <p><strong>Assessment:</strong> {alert.assessment}</p>
                    <p><strong>Recommendation:</strong> {alert.recommendation}</p>
                    <div>
                        <Button className="color-white bg-red-500 hover:bg-red-600" 
                                    onClick={() => handleAction('dismiss')}>
                            Dismiss
                        </Button>
                        <Button className="color-white bg-yellow-500 hover:bg-yellow-600 ml-2"
                                    onClick={() => handleAction('escalate')}>
                            Escalate
                        </Button>                        
                    </div>
                    
                </div>
            </DialogContent>
        </Dialog>
    );
}