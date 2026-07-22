import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { User } from "../types/user";
import type { AuthSession } from "../types/auth";

interface AuthUserState {
  user: User | null;
  accessToken: string | null;
  expiresAt: number | null;
  isAuthenticated: boolean;
}

const initialState: AuthUserState = {
  user: null,
  accessToken: null,
  expiresAt: null,
  isAuthenticated: false,
};

const authUserSlice = createSlice({
  name: "authUser",
  initialState,
  reducers: {
    setSession: (state, action: PayloadAction<AuthSession>) => {
      state.user = action.payload.user;
      state.accessToken = action.payload.accessToken;
      state.expiresAt = action.payload.expiresAt;
      state.isAuthenticated = true;
    },

    clearSession: (state) => {
      state.user = null;
      state.accessToken = null;
      state.expiresAt = null;
      state.isAuthenticated = false;
    },
  },
});

export const { setSession, clearSession } = authUserSlice.actions;

export default authUserSlice.reducer;