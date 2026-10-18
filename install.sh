#!/usr/bin/env bash
set -euo pipefail

# Usage: sudo bash install.sh template [port]
app_name="${1:-}"
port="${2:-5000}"
if [[ $# -lt 1 || $# -gt 2 || ! "$app_name" =~ ^[a-z0-9][a-z0-9-]*$ || ! "$port" =~ ^[0-9]{1,5}$ ]]; then
    echo "Usage: sudo bash install.sh <app-name> [port]" >&2
    exit 1
fi
port=$((10#$port))
if (( port < 1 || port > 65535 )); then
    echo "Port must be between 1 and 65535." >&2
    exit 1
fi
if (( EUID != 0 )); then
    echo "Run this installer with sudo." >&2
    exit 1
fi

# Existing source/package installations need an explicit data migration.
if [[ -e "/etc/systemd/system/$app_name.service" || -e "/lib/systemd/system/aiursoft-$app_name.service" ]]; then
    echo "An existing systemd installation was found. Migrate its configuration and data before using Docker." >&2
    exit 1
fi

if ! command -v docker >/dev/null 2>&1; then
    . /etc/os-release
    case " ${ID:-} ${ID_LIKE:-} " in
        *" debian "*|*" ubuntu "*|*" anduinos "*)
            apt-get update
            apt-get install -y docker.io
            ;;
        *" rhel "*|*" centos "*|*" fedora "*)
            # Docker maintains the distribution-specific repository setup.
            dnf install -y ca-certificates curl
            docker_installer=$(mktemp)
            trap 'rm -f "$docker_installer"' EXIT
            curl --fail --silent --show-error --location https://get.docker.com -o "$docker_installer"
            sh "$docker_installer"
            ;;
        *)
            echo "Install Docker Engine for this distribution, then run this script again." >&2
            exit 1
            ;;
    esac
    systemctl enable --now docker
fi

# Reuse an existing engine without reinstalling it or changing its configuration.
if ! docker info >/dev/null 2>&1; then
    echo "Docker Engine is not available. Start it and check access before retrying." >&2
    exit 1
fi
if docker container inspect "$app_name" >/dev/null 2>&1; then
    echo "Container '$app_name' already exists. It has been left unchanged." >&2
    exit 1
fi

image="docker.io/aiursoft/$app_name:latest"
docker pull "$image"
docker run --detach --name "$app_name" --restart unless-stopped \
    --publish "$port:5000" \
    --mount "type=volume,source=$app_name-data,target=/data" \
    "$image"

echo "Container created. Open http://localhost:$port after the application starts."
echo "Configuration and data are stored in the '$app_name-data' Docker volume."
echo "View startup logs with: sudo docker logs $app_name"
