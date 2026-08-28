
import { useAuth } from "@/features/login/hooks/useAuth";
import type { RootState } from "@/store/store";
import { useSelector } from "react-redux";
import { useLocation, Link } from "react-router-dom";
export function Header() {
    const {logout, user } = useAuth();
    const location = useLocation()
    const alerts = useSelector((state: RootState) => state.alert.alerts.length);
    return (
        <header className='w-full border border-blue-500'>
            <div className='border border-red-500 w-full grid grid-cols-11 gap-4 p-2 items-center'>
                <div className='text-sm border border-blue-500 col-span-9'>
                    <h2>Fleet Pulse Monitor </h2>
                </div>
                <div className='text-xs border border-blue-500 col-span-2 flex flex-row justify-between items-center'>
                    <div className="flex flex-col justify-start items-start border border-green-500">
                        <p>Logged in as {user ? user.username : "N/A"}</p>
                        <p>Recent alerts: ({alerts})</p>
                    </div>
                    <div className="flex justify-end border border-green-500">
                        <button className='bg-gray-500 text-white rounded px-1 py-1 hover:bg-blue-600 text-sm'
                        onClick={() => {
                            logout();
                        }}> Logout
                        </button>
                    </div>
                </div>
            </div>
            
            <div className='w-full border border-green-500'>
                <ul className="flex flex-row justify-start gap-4 p-2">
                    <li><Link to="/" className="text-black hover:underline">Home</Link></li>
                    <li><Link to="/alerts" className="text-black hover:underline">Alerts</Link></li>
                    <li><Link to="/drivers" className="text-black hover:underline">Drivers</Link></li>
                </ul>
            </div>
        </header>
    );
}