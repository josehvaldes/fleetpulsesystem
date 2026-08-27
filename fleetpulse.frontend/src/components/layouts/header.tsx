
import { useAuth } from "@/features/login/hooks/useAuth";
import type { RootState } from "@/store/store";
import { useSelector } from "react-redux";
export function Header() {
    const {logout, user } = useAuth();
    const alerts = useSelector((state: RootState) => state.alert.alerts.length);
    return (
        <header className='border border-red-500 w-full grid grid-cols-7 gap-4 p-2 justify-stretch items-center'>
            <div className='text-xs border border-blue-500 col-span-1'>
                <p>Logged in as {user ? user.username : "N/A"}</p>
                <p>Recent alerts: ({alerts})</p>
            </div>
            <div className='text-sm border border-blue-500 col-span-5'>
                <h2>GPS Ping Monitor </h2>
            </div>
            <div className= 'text-sm border border-blue-500 col-span-1'>
                <button className='bg-blue-500 text-white rounded px-2 py-2 hover:bg-blue-600 text-sm'
                onClick={() => {
                    logout();
                }}> Logout
                </button>
            </div>
        </header>
    );
}