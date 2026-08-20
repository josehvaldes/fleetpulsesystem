# Notes for Agents

Use Dapper for database access.


```sql
-- AI Alerts table
CREATE TABLE IF NOT EXISTS fleetpulse.alerts (
    id               UUID PRIMARY KEY DEFAULT uuidv7(),
    driver_id        VARCHAR(50) NOT NULL,
    
    event_latitude   DOUBLE PRECISION NOT NULL,
    event_longitude  DOUBLE PRECISION NOT NULL,
    exit_speed       DOUBLE PRECISION NOT NULL,
    exit_time        TIMESTAMPTZ,
    zone_name        VARCHAR(50) NOT NULL,
    zone_type        VARCHAR(50) NOT NULL,
    risk_level       VARCHAR(20) NOT NULL,
    assessment       TEXT NOT NULL,
    recommendation   TEXT NOT NULL,
    status           TEXT NOT NULL,
    autoscale        BOOLEAN,    
    raised_at        TIMESTAMPTZ,
    created_at       TIMESTAMPTZ DEFAULT NOW(),
    CONSTRAINT chk_jobs_status
        CHECK (status IN ('New', 'InProgress', 'Resolved', 'Closed', 'OnError'))
);
```