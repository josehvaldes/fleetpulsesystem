# Architecture Decision Records 0002

**The Database Ingestion Pipeline (DB Batch Writer)**

Writing 250 individual GPS pings per second directly to a relational database causes severe I/O bottlenecks and lock contention. The DB Writer solves this using a 3-stage pipeline:

Micro-Batching: The worker consumes the Kafka stream but holds GPS pings in an in-memory buffer, flushing to the database in bulk every 5 seconds (or at 1,000 records). This turns 250 I/O ops/sec into ~25 bulk ops/sec.
Temporal Compression: Before flushing, the logic drops redundant data. If a driver is stopped at a red light for 30 seconds, only the first and last ping are kept. Moving highway pings are down-sampled to 1 point per 15 seconds.
Dual-Table Write Strategy:
gps_history (Hypertable): Receives the bulk-inserted, compressed historical data. TimescaleDB automatically compresses data older than 7 days to save ~90% disk space.
driver_latest_state (Standard Table): Receives a continuous UPSERT. This table strictly contains exactly one row per active driver (e.g., 500 rows), allowing the frontend to query current locations in milliseconds without scanning millions of historical rows.
