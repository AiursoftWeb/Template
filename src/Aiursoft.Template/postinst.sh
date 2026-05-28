#!/bin/sh
set -e
mkdir -p /var/lib/aiursoft-template
chown www-data:www-data /var/lib/aiursoft-template

# Replace /usr/share copy with symlink to /etc
rm -f /usr/share/aiursoft-template/appsettings.json
ln -sf /etc/aiursoft-template/appsettings.json /usr/share/aiursoft-template/appsettings.json
