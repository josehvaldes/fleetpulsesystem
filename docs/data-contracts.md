# data-contracts.md




## Repository Structure

** Reference: ./db/init.sql **

** Database Table definition for GpsPing **
```sql
-- Hypertable for historical GPS data
CREATE TABLE IF NOT EXISTS fleetpulse.gps_history ();

-- Latest state table (one row per driver)
CREATE TABLE IF NOT EXISTS fleetpulse.driver_latest_state (
    status         VARCHAR(20) DEFAULT 'moving'  -- moving, stopped, offline
    CONSTRAINT chk_driver_state_status
        CHECK (status IN ('moving', 'stopped', 'offline'))
);
```

** Alert defintion in database
```sql
-- AI Alerts table
CREATE TABLE IF NOT EXISTS fleetpulse.alerts (
    id               UUID PRIMARY KEY DEFAULT uuidv7(),
    ...
    status           VARCHAR(20) NOT NULL
    CONSTRAINT chk_jobs_status
        CHECK (status IN ('New', 'InProgress', 'Resolved', 'Closed', 'OnError'))
);
```


## Data Models
** MQTT and Kafka message model

```JSON
	message = {
		"driver_id": string
		"timestamp": "datetime - isoformat",
		"latitude": "float",
		"longitude": "float",
		"speed_kmh": "int",
		"heading_degrees": "float",
		"accuracy_meters": "float",
		"status": string "decelerating" else "moving",
		"vehicle_type": string,
	}
```

** Alert payload**
```JSON
    {
        "driver_id": string,
        "exit_location": string,
        "exit_speed": float,
        "exit_heading": float,
        "exit_time": string,
        "zone_name": string,
        "zone_type": string,
        "agent_risk_level": string,
        "agent_assessment": string,
        "agent_recommendation": string,
        "agent_auto_escalate": bool,
        "created_at": string,
    }
```

*** Headers Metadata for open telemetry tracing
```JSON
    metadata = {
        "traceparent": string,
        "tracestate": string
    }
```
