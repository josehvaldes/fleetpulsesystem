import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { Alert } from "@/types/alert";
const liveAlertLimit = 20
interface AlertState {
  alerts: Alert[];
}

const initialState: AlertState = {
  alerts: [],
};

const alertSlice = createSlice({
  name: "alert",
  initialState,
  reducers: {
    addAlert(state, action: PayloadAction<Alert>) {
      
      state.alerts.push(action.payload);
      if (state.alerts.length > liveAlertLimit) {
        state.alerts.shift();
      }

    },
    removeAlert(state, action: PayloadAction<string>) {
      state.alerts = state.alerts.filter(alert => alert.id !== action.payload);
    },
  },
});

export const { addAlert, removeAlert } = alertSlice.actions;
export default alertSlice.reducer;