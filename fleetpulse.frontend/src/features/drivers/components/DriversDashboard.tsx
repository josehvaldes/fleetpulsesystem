import { Header } from "@/components/layouts/header"
import DriversMfeWrapper from "@/components/mfe/DriversMfeWrapper";
import { store } from "@/store/store";
import React from 'react';
import { config } from "@/utils/appConfig";

export function DriversDashboard() {

    // Assuming you store the token in Redux
    const token = store.getState().auth.accessToken;

    // Expose the getToken method as described in frontend.md
    const getAuthToken = React.useCallback(() => {
    return token;
    }, [token]);

    const apiBaseUrl = config.api.baseUrl || "https://localhost:7234/api";
    
    return (
        <>
        <div>
            <Header />
            <div className="drivers-dashboard border border-blue-500 p-2">
                <h3 className="font-bold">Drivers Dashboard</h3>
                <p>This is the Drivers Dashboard page.</p>
                <DriversMfeWrapper 
                    apiBaseUrl={apiBaseUrl} 
                    getAuthToken={getAuthToken} />
            </div>
        </div>
        </>
    );
}