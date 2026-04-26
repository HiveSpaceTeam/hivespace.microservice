#!/usr/bin/env bash
# Syncs all service appsettings.json / local.settings.json files
# from src/ into the shared hivespace.config repository.
#
# Run from the hivespace.microservice repo root:
#   bash scripts/sync-config.sh

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONFIG_ROOT="$(cd "$REPO_ROOT/../hivespace.config/hivespace.microservice" && pwd)"

sync_file() {
    local SRC="$1"
    local DST="$2"
    if [[ -f "$SRC" ]]; then
        mkdir -p "$(dirname "$DST")"
        cp "$SRC" "$DST"
        echo "  synced: $(basename "$SRC")  →  ${DST#$CONFIG_ROOT/}"
    else
        echo "  skip (not found): $SRC"
    fi
}

echo "Syncing config files to hivespace.config..."
echo ""

sync_file "$REPO_ROOT/src/HiveSpace.ApiGateway/HiveSpace.YarpApiGateway/appsettings.json" \
          "$CONFIG_ROOT/apigateway/appsettings.json"

sync_file "$REPO_ROOT/src/HiveSpace.CatalogService/HiveSpace.CatalogService.Api/appsettings.json" \
          "$CONFIG_ROOT/services/catalog/appsettings.json"

sync_file "$REPO_ROOT/src/HiveSpace.MediaService/HiveSpace.MediaService.Api/appsettings.json" \
          "$CONFIG_ROOT/services/media/appsettings.json"

sync_file "$REPO_ROOT/src/HiveSpace.MediaService/HiveSpace.MediaService.Func/local.settings.json" \
          "$CONFIG_ROOT/services/media/local.settings.json"

sync_file "$REPO_ROOT/src/HiveSpace.NotificationService/HiveSpace.NotificationService.Api/appsettings.json" \
          "$CONFIG_ROOT/services/notification/appsettings.json"

sync_file "$REPO_ROOT/src/HiveSpace.OrderService/HiveSpace.OrderService.Api/appsettings.json" \
          "$CONFIG_ROOT/services/order/appsettings.json"

sync_file "$REPO_ROOT/src/HiveSpace.PaymentService/HiveSpace.PaymentService.Api/appsettings.json" \
          "$CONFIG_ROOT/services/payment/appsettings.json"

sync_file "$REPO_ROOT/src/HiveSpace.UserService/HiveSpace.UserService.Api/appsettings.json" \
          "$CONFIG_ROOT/services/user/appsettings.json"

echo ""
echo "Done. Commit changes in hivespace.config if any files changed."
