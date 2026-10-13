#!/usr/bin/env bash
# Prepared for administrator review. Not executed by Codex.
# Requires an admitted .NET 10 runtime and separate public development-release authority.
set -euo pipefail
if [[ ${1:-} != --activate-development-download ]]; then
    echo "Usage: sudo bash activate-vps-development.sh --activate-development-download"
    echo "Publishes the reviewed development portal and unsigned manual-download package. Not Production approval."
    exit 2
fi
[[ $EUID -eq 0 ]] || { echo "Administrator privileges required."; exit 2; }
stage=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)
cd "$stage"
sha256sum --check SHA256SUMS
[[ -z $(find web releases config -type l -print -quit) ]] || { echo "Symlinks in staging rejected."; exit 2; }
[[ -x /usr/bin/dotnet ]] && /usr/bin/dotnet --list-runtimes | grep -q '^Microsoft.AspNetCore.App 10\.' || {
    echo "Install the admitted ASP.NET Core 10 runtime first; see SERVER-NEXT-STEPS.txt."; exit 2;
}
site=/etc/nginx/sites-available/oto.ayani.id
expected=7ca6b06b4911ae5b094c025ce13f7be87365ab6e95b7d19d42ddd965da706fba
[[ $(sha256sum "$site" | cut -d' ' -f1) == "$expected" ]] || { echo "Existing Nginx configuration changed; review again."; exit 2; }
[[ ! -e /etc/systemd/system/oto-downloads.service && ! -e /opt/oto-downloads && ! -e /var/lib/oto-downloads ]] || {
    echo "An OTO deployment already exists. Use a separately reviewed upgrade plan."; exit 2;
}
if ss -ltn | grep -q ':5088 '; then echo "Port 5088 already occupied."; exit 2; fi
nginx -t
build=20261001-094532-c58e4ca7
backup=/var/backups/oto-downloads/$build
install -d -m 0700 "$backup"
cp -p -- "$site" "$backup/oto.ayani.id.before.conf"
changed_site=0
started_service=0
rollback() {
    echo "Activation failed. Restoring the previous site configuration."
    if [[ $changed_site == 1 ]]; then cp -p -- "$backup/oto.ayani.id.before.conf" "$site"; nginx -t && systemctl reload nginx; fi
    if [[ $started_service == 1 ]]; then systemctl stop oto-downloads.service; fi
    echo "Deployment files retained for review; no other site was changed. Backup: $backup"
}
trap rollback ERR
if id oto-web >/dev/null 2>&1; then
    [[ $(id -u oto-web) -gt 0 && $(id -u oto-web) -lt 1000 ]] || { echo "Unexpected oto-web account."; exit 2; }
else
    useradd --system --user-group --no-create-home --shell /usr/sbin/nologin oto-web
fi
install -d -m 0755 /opt/oto-downloads /var/lib/oto-downloads
install -d -m 0750 -o root -g oto-web "/opt/oto-downloads/$build" /var/lib/oto-downloads/releases
cp -R -- web/. "/opt/oto-downloads/$build/"
cp -R -- releases/. /var/lib/oto-downloads/releases/
chown -R root:oto-web "/opt/oto-downloads/$build" /var/lib/oto-downloads/releases
find "/opt/oto-downloads/$build" /var/lib/oto-downloads/releases -type d -exec chmod 0750 {} +
find "/opt/oto-downloads/$build" /var/lib/oto-downloads/releases -type f -exec chmod 0640 {} +
ln -s "/opt/oto-downloads/$build" /opt/oto-downloads/current
install -m 0644 config/oto-downloads.service /etc/systemd/system/oto-downloads.service
systemctl daemon-reload
started_service=1
systemctl start oto-downloads.service
ready=0
for attempt in {1..30}; do
    if [[ $(curl -s -o /dev/null -w '%{http_code}' --max-time 2 http://127.0.0.1:5088/health || true) == 204 ]]; then ready=1; break; fi
    sleep 1
done
[[ $ready == 1 ]]
changed_site=1
install -m 0644 config/oto.ayani.id.vps-reviewed.conf "$site"
nginx -t
systemctl reload nginx
[[ $(curl -s -o /dev/null -w '%{http_code}' --max-time 10 https://oto.ayani.id/health) == 204 ]]
curl -fsS --max-time 15 https://oto.ayani.id/api/releases/windows-x64/latest > "$backup/activated-release.json"
grep -q "$build" "$backup/activated-release.json"
systemctl enable oto-downloads.service
trap - ERR
echo "Development portal active. Verify the actual download and installed-client notification before acceptance."
echo "Nginx backup: $backup/oto.ayani.id.before.conf"
