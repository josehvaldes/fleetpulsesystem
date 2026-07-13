#!/bin/sh
# This script is used to bootstrap the EMQX configuration for the FleetPulseSystem project.
# PARAMS:
BOOTSTRAP_HOST="redpanda-0:9092"
REDPANDA_TOPIC="gps-pings"
EMQX_TOPIC="fleet_pulse"

# Wait for EMQX API to become available
echo "Waiting for EMQX API..."
while ! curl -s http://emqx:18083/api/v5/status | grep -q "is running"; do 
  sleep 4
done
echo "EMQX is up! Logging in..."

# 1. Log in to get the JWT token
LOGIN_RESPONSE=$(curl -s -X POST 'http://emqx:18083/api/v5/login' \
  -H 'Content-Type: application/json' \
  -d '{"username": "admin", "password": "emqx!@#$"}')

# Extract the token (using simple shell commands available in the curl image)
TOKEN=$(echo "$LOGIN_RESPONSE" | grep -o '"token":"[^"]*' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
  echo "Failed to get auth token. Response was: $LOGIN_RESPONSE"
  exit 1
fi

echo "Token acquired. Deploying configurations..."

# 2. Create the Connector (using Bearer Auth)
curl -s -X POST 'http://emqx:18083/api/v5/connectors' \
-H "Authorization: Bearer $TOKEN" \
-H 'Content-Type: application/json' \
-d "{  \"name\": \"to_redpanda\",  \"type\": \"kafka_producer\",  \"enable\": true,  \"bootstrap_hosts\": \"$BOOTSTRAP_HOST\"}"

echo "Connector created. "
# 3. Create the Action
curl -s -X POST 'http://emqx:18083/api/v5/actions' \
-H "Authorization: Bearer $TOKEN" \
-H 'Content-Type: application/json' \
-d "{  \"name\": \"to_gps_pings\",  \"type\": \"kafka_producer\",  \"enable\": true,  \"connector\": \"to_redpanda\", \"description\": \"Action to send messages to Redpanda\", \"parameters\": { \"topic\": \"$REDPANDA_TOPIC\"  } }"

echo "Action created. Creating Rule..."
# 4. Create the Rule
curl -s -X POST 'http://emqx:18083/api/v5/rules' \
-H "Authorization: Bearer $TOKEN" \
-H 'Content-Type: application/json' \
-d "{  \"id\": \"fleet_pulse_to_kafka_rule\",  \"name\": \"fleet_pulse_to_kafka_rule\",  \"enable\": true,  \"sql\": \"SELECT * FROM \\\"$EMQX_TOPIC/#\\\"\",  \"actions\": [\"kafka_producer:to_gps_pings\"]}"

echo -e "\nDeployment complete."