
import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { login as loginService, logout as logoutService } from "../services/loginService";
import { clearSession, hydrateSession, setSession } from "../store/authUserSlice";
import type { AppDispatch, RootState } from "../store/store";
import { getStoredAuthSession } from "../utils/authStorage";

export function useAuth() {
  const dispatch = useDispatch<AppDispatch>();
  const auth = useSelector((state: RootState) => state.auth);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (auth.isHydrated) {
      return;
    }

    dispatch(hydrateSession(getStoredAuthSession()));
  }, [auth.isHydrated, dispatch]);

  const login = async (username: string, password: string) => {
    setIsLoading(true);

    try {
      const session = await loginService(username.trim(), password);
      dispatch(setSession(session));
      return session;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    setIsLoading(true);

    try {
      await logoutService();
      dispatch(clearSession());
    } finally {
      setIsLoading(false);
    }
  };

  return {
    ...auth,
    isLoading,
    login,
    logout,
  };
}


