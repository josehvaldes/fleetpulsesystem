
import { InteractionStatus } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import { apiScopes } from "@/utils/apiScopes";

export function useMsalAuth() {
    const { instance, accounts, inProgress } = useMsal();
    const account = instance.getActiveAccount() ?? accounts[0] ?? null;
    const isAuthenticated = account !== null;
    const isLoading = inProgress !== InteractionStatus.None;

    const login = async () => {
        try 
        {
            console.log("Starting login process...");
            const response = await instance.loginPopup({ scopes: apiScopes });

            if (response.account) {
                instance.setActiveAccount(response.account);
            }

        } catch (error) {
            console.error("Login failed:", error);
            throw error;
        }
        
    };

    const logout = async () => {
        await instance.logoutPopup({ account: account ?? undefined });
    };

    const getAuthToken = async (): Promise<string | null> => {
        if (account === null) {
            return null;
        }

        const response = await instance.acquireTokenSilent({
            account,
            scopes: apiScopes,
        });
        return response.accessToken;
    };

    return { account, isAuthenticated, isLoading, login, logout, getAuthToken };
}