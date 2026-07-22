import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { User } from "../types/user";
import type { AuthSession } from "../utils/authStorage";

interface AuthUserState {
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  isHydrated: boolean;
}

const initialState: AuthUserState = {
  user: null,
  accessToken: null,
  isAuthenticated: false,
  isHydrated: false,
};

const authUserSlice = createSlice({
  name: "authUser",
  initialState,
  reducers: {
    hydrateSession: (state, action: PayloadAction<AuthSession | null>) => {
      if (action.payload) {
        state.user = action.payload.user;
        state.accessToken = action.payload.accessToken;
        state.isAuthenticated = true;
      } else {
        state.user = null;
        state.accessToken = null;
        state.isAuthenticated = false;
      }

      state.isHydrated = true;
    },

    setSession: (state, action: PayloadAction<AuthSession>) => {
      state.user = action.payload.user;
      state.accessToken = action.payload.accessToken;
      state.isAuthenticated = true;
      state.isHydrated = true;
    },

    clearSession: (state) => {
      state.user = null;
      state.accessToken = null;
      state.isAuthenticated = false;
      state.isHydrated = true;
    },
  },
});

export const { hydrateSession, setSession, clearSession } = authUserSlice.actions;

export default authUserSlice.reducer;