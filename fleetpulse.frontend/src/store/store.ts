import { configureStore } from "@reduxjs/toolkit";
import authUserReducer from "./authUserSlice";

export const store = configureStore({
  reducer: {
    auth: authUserReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;