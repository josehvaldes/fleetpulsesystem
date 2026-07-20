import { useEffect } from "react";
import { MapContainer, TileLayer, Marker, Popup, useMap } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import L from "leaflet";
import { useGpsPings } from "../hooks/useGpsPings";

// Fix for default marker icons in Vite/Webpack environments
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: "https://unpkg.com/leaflet@1.7.1/dist/images/marker-icon-2x.png",
  iconUrl: "https://unpkg.com/leaflet@1.7.1/dist/images/marker-icon.png",
  shadowUrl: "https://unpkg.com/leaflet@1.7.1/dist/images/marker-shadow.png",
});

function ResizeMap() {
  const map = useMap();

  useEffect(() => {
    map.invalidateSize();
  }, [map]);

  return null;
}

export function FleetMap() {
  const { drivers, connected } = useGpsPings();

  return (
    <div>
      <h2>
        Fleet Map{" "}
        <span style={{ color: connected ? "green" : "red" }}>
          {connected ? "● Live" : "○ Connecting"}
        </span>
      </h2>
      
      <div className="relative w-full h-[60vh] overflow-hidden border border-gray-300">
        <MapContainer
          //center={[40.4168, -3.7038]}
          center={[-17.378384, -66.151922]}
          zoom={13}
          scrollWheelZoom={true}
          className="h-full w-full"
          style={{ height: "100%", width: "100%" }}
        >
          <ResizeMap />
          {/* OpenStreetMap Tiles */}
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          
          {/* Render a marker for each driver */}
          {Object.values(drivers).map((ping) => (
            <Marker
              key={ping.driver_id}
              position={[ping.latitude, ping.longitude]}
            >
              <Popup>
                <strong>{ping.driver_id}</strong>
                <br />
                Speed: {ping.speed_kmh} km/h
                <br />
                Status: {ping.status}
              </Popup>
            </Marker>
          ))}
        </MapContainer>
      </div>
    </div>
  );
}