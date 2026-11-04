import { Header } from "@/components/layouts/header"
import DriversMfeWrapper from "@/components/mfe/DriversMfeWrapper";

export function DriversDashboard() {
  
    return (
        <>
        <div>
            <Header />
            <div className="drivers-dashboard border border-blue-500 p-2">
                <DriversMfeWrapper />
            </div>
        </div>
        </>
    );
}