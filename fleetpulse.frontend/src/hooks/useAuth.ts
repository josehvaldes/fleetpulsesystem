
import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { login as loginService} from "../services/loginService";
import { clearSession, setSession } from "../store/authUserSlice";
import type { AppDispatch, RootState } from "../store/store";

export function useAuth() {
  const dispatch = useDispatch<AppDispatch>();
  const auth = useSelector((state: RootState) => state.auth);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (auth.isAuthenticated && auth.expiresAt && auth.expiresAt <= Date.now()) {
      dispatch(clearSession());
    }
  }, [auth.isAuthenticated, auth.expiresAt, dispatch]);

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


