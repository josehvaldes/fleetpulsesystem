#!/bin/sh
set -eu

EMQX_HOST="http://emqx-emqx-enterprise:18083"
BOOTSTRAP_HOST="redpanda.fleetpulse.svc.cluster.local:9093"
REDPANDA_TOPIC="gps-pings"
EMQX_TOPIC="fleet_pulse"

: "${EMQX_USERNAME:?EMQX_USERNAME is required}"
: "${EMQX_PASSWORD:?EMQX_PASSWORD is required}"

echo "Waiting for EMQX API..."

until curl -fsS "$EMQX_HOST/api/v5/status" 2>/dev/null \
  | grep -q "is running"; do
  sleep 4
done

LOGIN_RESPONSE=$(curl -fsS \
  -X POST "$EMQX_HOST/api/v5/login" \
  -H 'Content-Type: application/json' \
  -d "{\"username\":\"$EMQX_USERNAME\",\"password\":\"$EMQX_PASSWORD\"}")

TOKEN=$(printf '%s' "$LOGIN_RESPONSE" \
  | grep -o '"token":"[^"]*' | cut -d '"' -f 4)

if [ -z "$TOKEN" ]; then
  echo "Failed to obtain EMQX token"
  exit 1
fi

echo "EMQX is ready. Configuring integration..."

# Continue with your existing connector, action, and rule requests.
# Keep the payloads and resource names from your current script.
# Add --fail-with-body to each curl request so HTTP errors fail visibly.

# 2. Create the Connector (using Bearer Auth)
curl -s -X POST "$EMQX_HOST/api/v5/connectors" \
-H "Authorization: Bearer $TOKEN" \
-H 'Content-Type: application/json' \
-d "{  \"name\": \"to_redpanda\",  \"type\": \"kafka_producer\",  \"enable\": true,  \"bootstrap_hosts\": \"$BOOTSTRAP_HOST\"}"

echo "Connector created. "
# 3. Create the Action
curl -s -X POST "$EMQX_HOST/api/v5/actions" \
-H "Authorization: Bearer $TOKEN" \
-H 'Content-Type: application/json' \
-d "{  \"name\": \"to_gps_pings\",  \"type\": \"kafka_producer\",  \"enable\": true,  \"connector\": \"to_redpanda\", \"description\": \"Action to send messages to Redpanda\",  \"parameters\": { \"topic\": \"$REDPANDA_TOPIC\", \"partition_strategy\": \"key_dispatch\",  \"kafka_ext_headers\": [ { \"kafka_ext_header_key\": \"traceparent\",   \"kafka_ext_header_value\": \"\${traceparent}\"}, { \"kafka_ext_header_key\": \"tracestate\", \"kafka_ext_header_value\": \"\${tracestate}\" } ], \"message\":{\"key\":\"\${kafka_key}\", \"value\": \"\${payload}\"  }  } }" 

echo "Action created. Creating Rule..."
# 4. Create the Rule
curl -s -X POST "$EMQX_HOST/api/v5/rules" \
-H "Authorization: Bearer $TOKEN" \
-H 'Content-Type: application/json' \
-d "{  \"id\": \"fleet_pulse_to_kafka_rule\",  \"name\": \"fleet_pulse_to_kafka_rule\",  \"enable\": true,  \"sql\": \"SELECT payload.driver_id as kafka_key, payload, pub_props.'User-Property'.traceparent as traceparent, pub_props.'User-Property'.tracestate as tracestate FROM \\\"$EMQX_TOPIC/#\\\"\",  \"actions\": [\"kafka_producer:to_gps_pings\"]}"


echo "EMQX bootstrap completed."