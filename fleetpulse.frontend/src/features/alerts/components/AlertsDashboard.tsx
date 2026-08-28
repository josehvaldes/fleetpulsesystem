import { Header } from "@/components/layouts/header"



export function AlertsDashboard() {
    return (
        <>
        <div>
            <Header />
            <div className="alerts-dashboard border border-blue-500 p-2">
                <h3 className="font-bold">Alerts Dashboard</h3>
                <p>This is the Alerts Dashboard page.</p>
            </div>
        </div>
        </>
    );
}