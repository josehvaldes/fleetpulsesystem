import { createContext, createElement, useContext, type ReactNode } from "react";
import type { User } from "@/types/user";

interface AuthContextValue {
    logout: () => Promise<void>;
    user: User | null;
}

interface AuthContextProviderProps {
    children: ReactNode;
    value: AuthContextValue;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthContextProvider({ children, value }: AuthContextProviderProps) {
    return createElement(AuthContext.Provider, { value }, children);
}

export function useAuthContext(): AuthContextValue {
    const context = useContext(AuthContext);

    if (context === null) {
        throw new Error("useAuthContext must be used inside an AuthContextProvider.");
    }

    return context;
}