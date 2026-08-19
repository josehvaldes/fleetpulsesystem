-- Enable TimescaleDB extension
CREATE EXTENSION IF NOT EXISTS timescaledb;

-- Hypertable for historical GPS data
CREATE TABLE gps_history (
    event_id       UUID NOT NULL DEFAULT uuidv7(),
    driver_id      VARCHAR(50) NOT NULL,
    timestamp      TIMESTAMPTZ NOT NULL,
    latitude       DOUBLE PRECISION NOT NULL,
    longitude      DOUBLE PRECISION NOT NULL,
    speed          DOUBLE PRECISION,
    heading        INTEGER,
    accuracy       DOUBLE PRECISION,
    raw_payload    JSONB
);

SELECT create_hypertable('gps_history', 'timestamp');


-- Compression policy (older than 7 days)
ALTER TABLE gps_history SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'driver_id'
);

SELECT add_compression_policy('gps_history', INTERVAL '7 days');

-- Latest state table (one row per driver)
CREATE TABLE driver_latest_state (
    driver_id      VARCHAR(50) PRIMARY KEY,
    latitude       DOUBLE PRECISION NOT NULL,
    longitude      DOUBLE PRECISION NOT NULL,
    speed          DOUBLE PRECISION,
    heading        INTEGER,
    last_seen      TIMESTAMPTZ NOT NULL,
    status         VARCHAR(20) DEFAULT 'moving'  -- moving, stopped, offline
);

-- AI Alerts table
CREATE TABLE alerts (
    id               UUID PRIMARY KEY DEFAULT uuidv7(),
    driver_id        VARCHAR(50) NOT NULL,
    
    event_latitude   DOUBLE PRECISION NOT NULL,
    event_longitude  DOUBLE PRECISION NOT NULL,
    exit_speed       DOUBLE PRECISION NOT NULL,
    exit_time        TIMESTAMPTZ,
    zone_name        VARCHAR(50) NOT NULL,
    zone_type        VARCHAR(50) NOT NULL,
    risk_level       VARCHAR(50) NOT NULL,
    assessment       VARCHAR(50) NOT NULL,
    recommendation   VARCHAR(50) NOT NULL,
    autoscale        BOOLEAN,
    raised_at        TIMESTAMPTZ,
    created_at       TIMESTAMPTZ DEFAULT NOW()
);

-- Indexes for common queries
CREATE INDEX idx_gps_history_event_id ON gps_history (event_id);
CREATE INDEX idx_gps_history_driver_time ON gps_history (driver_id, timestamp DESC);
CREATE INDEX idx_ai_alerts_driver ON ai_alerts (driver_id, created_at DESC);
CREATE INDEX idx_ai_alerts_severity ON ai_alerts (severity, created_at DESC);