import { useGpsPings } from "../hooks/useGpsPings";
import { statusStyle } from "../utils/fleethub_utils";

export function MessageLog() {
  const { pings, status } = useGpsPings();
  const statusUi = statusStyle(status);

  // One line per ping, formatted for didactic readability.
  const text = pings
    .map((p) =>
      `${p.timestamp} | ${p.driver_id} | ` +
      `lat=${p.latitude.toFixed(6)} lng=${p.longitude.toFixed(6)} | ` +
      `${p.speed_kmh} km/h | ${p.status}`
    )
    .join("\n");

  return (
    <div className="border border-blue-500 p-2">
      <h2>
        Live GPS Pings{" "}
        <span style={{ color: statusUi.color }}>
          {statusUi.label}
        </span>
      </h2>
      <div className="">
        <textarea
            className=""
            readOnly
            value={text}
            placeholder="Waiting for messages from /v1/fleetHub…"
            style={{
            width: "100%",
            height: "20vh",
            fontFamily: "monospace",
            fontSize: 12,
            background: "#0b0f14",
            color: "#cdd6e0",
            border: "1px solid #333",
            padding: "0.5rem",
            }}
        />
      </div>
      <p>{pings.length} pings buffered (max 200).</p>
    </div>
  );
}