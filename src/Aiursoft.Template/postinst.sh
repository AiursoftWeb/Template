#!/bin/sh
set -e
mkdir -p /var/lib/aiursoft-template
chown www-data:www-data /var/lib/aiursoft-template

# Bootstrap config if missing (matching Docker entrypoint pattern)
if [ ! -f /etc/aiursoft-template/appsettings.json ]; then
    cp /usr/share/aiursoft-template/appsettings.json /etc/aiursoft-template/appsettings.json
fi
# Replace /usr/share copy with symlink to /etc
rm -f /usr/share/aiursoft-template/appsettings.json
ln -sf /etc/aiursoft-template/appsettings.json /usr/share/aiursoft-template/appsettings.json
