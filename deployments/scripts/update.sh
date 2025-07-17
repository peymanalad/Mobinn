#!/bin/sh
set -e

# Load .env file into environment
if [ -f ".env" ]; then
  export $(grep -v '^#' .env | xargs)
fi

HEALTHCHECK_TIMEOUT=40
NO_HEALTHCHECK_TIMEOUT=10
WAIT_AFTER_HEALTHY_DELAY=40
SERVICE=backend

ENVIRONMENT=$1
COMPOSE_FILE=$2

healthcheck() {
    echo "==> HEALTH CHECKING CONTAINER: $1"
    docker inspect --format='{{json .State.Health.Status}}' "$1" | grep -v "unhealthy" | grep -q "healthy"
}

main() {
    docker compose -f "$COMPOSE_FILE" up -d

    CONTAINER_ID=$(docker compose -f "$COMPOSE_FILE" ps -q "$SERVICE" 2>/dev/null)
    CONTAINER_NAME=$(docker inspect --format '{{.Name}}' "$CONTAINER_ID" 2>/dev/null | sed 's|/||g')


    echo "==> Waiting for container to be healthy (timeout: $HEALTHCHECK_TIMEOUT seconds)"
    for _ in $(seq 1 "$HEALTHCHECK_TIMEOUT"); do
        if healthcheck "$CONTAINER_NAME"; then
            echo "==> Container is healthy!"
            cp ./"$COMPOSE_FILE" "./backup_compose_file/$COMPOSE_FILE"
            cp ./.env "./backup_compose_file/.env"
            exit 0
        fi
        echo "==> Still waiting for container to become healthy..."
        sleep 1
    done

    echo "==> HEALTHCHECK ERROR" >&2
    echo "==> ROLLING BACK"
    docker logs "$CONTAINER_ID" > "$SERVICE-logs"
    docker compose -f "$COMPOSE_FILE" down
    cp "./backup_compose_file/$COMPOSE_FILE" ./"$COMPOSE_FILE"
    cp "./backup_compose_file/.env" ./.env
    docker compose -f "$COMPOSE_FILE" up -d
    cat "$SERVICE-logs"
    echo "==> Container is not healthy and rolled back to previous compose file"
    exit 1
}

main "$@"
