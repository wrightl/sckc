#!/usr/bin/env bash
# Runs the whole stack for local development and testing:
#   - the .NET API on http://localhost:32767
#   - the Angular dev server on http://localhost:4200 (proxying /api and gallery images to the API)
#   - an ngrok tunnel to the dev server, so other people can test it
#
# Usage: ./run.sh [--no-ngrok]
# Ctrl+C stops everything.

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_PORT=32767
APP_PORT=4200
USE_NGROK=true

for arg in "$@"; do
  case "$arg" in
    --no-ngrok) USE_NGROK=false ;;
    -h|--help) sed -n '2,9p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "Unknown option: $arg" >&2; exit 1 ;;
  esac
done

require() {
  command -v "$1" >/dev/null 2>&1 || { echo "Missing '$1' - please install it first." >&2; exit 1; }
}
require dotnet
require npx
$USE_NGROK && require ngrok

for port in $API_PORT $APP_PORT; do
  if lsof -nP -iTCP:"$port" -sTCP:LISTEN >/dev/null 2>&1; then
    echo "Port $port is already in use - stop whatever is running there first." >&2
    exit 1
  fi
done

pids=()
cleanup() {
  trap - INT TERM EXIT
  echo
  echo "Stopping..."
  for pid in ${pids[@]+"${pids[@]}"}; do
    # Kill each process group so child processes (e.g. the compiled API, esbuild) stop too
    kill -TERM -- "-$pid" 2>/dev/null || kill -TERM "$pid" 2>/dev/null || true
  done
  wait 2>/dev/null || true
}
trap cleanup INT TERM EXIT

# Prefix each line of output so the three logs can be told apart
prefix() {
  local label="$1"
  while IFS= read -r line; do printf '%s %s\n' "$label" "$line"; done
}

# Run each service in its own process group so cleanup can stop the whole tree
set -m

echo "Starting API on http://localhost:$API_PORT ..."
(cd "$ROOT/sckc/sckc.api" && exec dotnet run --launch-profile sckc.api --no-launch-browser 2>&1 | prefix "[api]") &
pids+=($!)

if [ ! -d "$ROOT/sckc-app/node_modules" ]; then
  echo "Installing app dependencies..."
  (cd "$ROOT/sckc-app" && npm install)
fi

echo "Starting Angular app on http://localhost:$APP_PORT ..."
(cd "$ROOT/sckc-app" && exec npx ng serve --configuration ngrok --port "$APP_PORT" 2>&1 | prefix "[app]") &
pids+=($!)

if $USE_NGROK; then
  # Wait for the dev server before opening the tunnel, so the first visitors don't get an ngrok error page
  echo "Waiting for the Angular app to be ready..."
  until curl -s -o /dev/null "http://localhost:$APP_PORT"; do
    sleep 2
  done

  echo "Starting ngrok tunnel..."
  ngrok http "$APP_PORT" --log stdout --log-format logfmt >/dev/null &
  pids+=($!)

  # ngrok's local API reports the public URL once the tunnel is up
  for _ in $(seq 1 15); do
    url="$(curl -s http://localhost:4040/api/tunnels | grep -o '"public_url":"https://[^"]*' | head -1 | cut -d'"' -f4 || true)"
    [ -n "$url" ] && break
    sleep 1
  done

  if [ -n "${url:-}" ]; then
    echo
    echo "=================================================="
    echo " Share this link: $url"
    echo " ngrok inspector: http://localhost:4040"
    echo "=================================================="
    echo
  else
    echo "ngrok started but its public URL couldn't be read - check http://localhost:4040" >&2
  fi
fi

echo "Everything is running. Press Ctrl+C to stop."
wait
