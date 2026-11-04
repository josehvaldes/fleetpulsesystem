

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


export function riskLevelStyle(riskLevel: string): { color: string; label: string } {
  switch (riskLevel.toLowerCase()) {
    case "low":
      return { color: "text-green-500", label: "● Low" };
    case "medium":
      return { color: "text-yellow-500", label: "● Medium" };
    case "high":
      return { color: "text-red-500", label: "● High" };
    default:
      return { color: "text-gray-500", label: "Unknown" };
  }
} 