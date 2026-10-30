
import { useEffect, type ReactNode } from "react";
import { AuthContextProvider } from "@/features/login/hooks/useAuthContext";
import { useMsalAuth } from "@/features/login/hooks/useMsalAuth";
import { setAuthTokenProvider } from "@/services/authTokenProvider";
import { Button } from "@/components/ui/button";

interface MsalAuthGateProps {
    children: ReactNode;
}

export function MsalAuthGate({ children }: MsalAuthGateProps) {
    const { account, 
        getAuthToken, 
        isAuthenticated, 
        isLoading, 
        login, 
        logout } = useMsalAuth();

    useEffect(() => {
        console.log("MsalAuthGate:");
        console.log("isAuthenticated: ", isAuthenticated);
        setAuthTokenProvider(isAuthenticated ? getAuthToken : null);

        return () => setAuthTokenProvider(null);
    }, [getAuthToken, isAuthenticated]);

    if (isAuthenticated) {
        
        const user = account === null
            ? null
            : { id: account.homeAccountId, username: account.username };

        return (
            <AuthContextProvider value={{ logout, user }}>
                {children}
            </AuthContextProvider>
        );
    }

    return (
        <div className="flex flex-col items-center justify-center min-h-screen">
            Please sign in to continue.
            <div >
                <Button className="mt-4"  onClick={login} disabled={isLoading}>
                    {isLoading ? "Signing in..." : "Login with MSAL"}
                </Button>
            </div>            
        </div>
    );
}