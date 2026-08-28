import { Header } from "@/components/layouts/header"



export function DriversDashboard() {
    return (
        <>
        <div>
            <Header />
            <div className="drivers-dashboard border border-blue-500 p-2">
                <h3 className="font-bold">Drivers Dashboard</h3>
                <p>This is the Drivers Dashboard page.</p>
            </div>
        </div>
        </>
    );
}