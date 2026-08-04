

export function statusStyle(status: string): { color: string; label: string } {
  switch (status) {
    case "connected":
      return { color: "green", label: "● Live" };
    case "reconnecting":
      return { color: "orange", label: "◐ Reconnecting" };
    case "connecting":
      return { color: "#b8860b", label: "◌ Connecting" };
    case "disconnected":
    default:
      return { color: "red", label: "○ Disconnected" };
  }
}

