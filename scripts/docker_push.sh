#!/bin/sh

set -eu

target="$1"
attempt=1

while [ "$attempt" -le 3 ]; do
    echo "Building and pushing multi-arch image to $target (attempt $attempt/3)..."
    if timeout --foreground 75m docker buildx build . \
        --platform linux/amd64,linux/arm64 \
        --tag "$target" \
        --push; then
        exit 0
    fi

    if [ "$attempt" -lt 3 ]; then
        echo "Push to $target failed or timed out. Retrying in 30 seconds..."
        sleep 30
    fi
    attempt=$((attempt + 1))
done

echo "Push to $target failed after 3 attempts."
exit 1
