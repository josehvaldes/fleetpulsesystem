#!/usr/bin/bash
VERSION="${1:-1.0}"
CLUSTER="fleetpulse"
VERSION="${1:-1.0}"

echo "Load docker image using version: $VERSION"

kind load docker-image "fleetpulse-api:${VERSION}" --name "${CLUSTER}"
kind load docker-image "fleetpulse-signalrhub:${VERSION}" --name "${CLUSTER}"
kind load docker-image "fleetpulse-ai-worker:${VERSION}" --name "${CLUSTER}"
kind load docker-image "fleetpulse-dbwriter:${VERSION}" --name "${CLUSTER}"

echo "Images successfully loaded"