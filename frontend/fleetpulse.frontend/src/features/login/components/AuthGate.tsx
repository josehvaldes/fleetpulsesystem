import type { ReactNode } from "react";
import { LocalAuthGate } from "./LocalAuthGate";
import { MsalAuthGate } from "./MsalAuthGate";

const authMode = import.meta.env.VITE_AUTH_MODE;

interface AuthGateProps {
    children: ReactNode;
}

export function AuthGate({ children }: AuthGateProps) {
    if (authMode === "local") {
        return <LocalAuthGate>{children}</LocalAuthGate>;
    }

    return <MsalAuthGate>{children}</MsalAuthGate>;
}