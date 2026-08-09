#!/usr/bin/env bash
# Bundle the PaperTodo mac shell into a loadable .app.
#
# The app must run from a bundle: a bare `dotnet` process is treated as a low-resolution
# app by macOS (Scaling=1, blurry rendering) because NSHighResolutionCapable is read from
# Info.plist. This script publishes the mac shell and wraps it in PaperTodo.app.
#
# Usage: scripts/bundle-mac.sh [debug|release]
set -euo pipefail
cd "$(dirname "$0")/.."

CONFIG="${1:-Debug}"
RID="${RID:-osx-arm64}"
SELF_CONTAINED="${SELF_CONTAINED:-true}"
APP="PaperTodo.app"
ROOT="$(pwd)"
OUT="$ROOT/输出/PaperTodo-mac"

if [ "$SELF_CONTAINED" = "true" ]; then
  SC_FLAG="--self-contained true"
else
  # Framework-dependent needs DOTNET_ROOT pointing at an installed SDK/runtime.
  SC_FLAG="--self-contained false"
fi

echo "==> publish ($CONFIG, $RID, self-contained=$SELF_CONTAINED)"
dotnet publish PaperTodo.Mac/PaperTodo.Mac.csproj \
  -c "$CONFIG" -r "$RID" $SC_FLAG \
  -p:PublishSingleFile=false \
  -o "$OUT/publish"

echo "==> assemble $APP"
APP_DIR="$OUT/$APP"
rm -rf "$APP_DIR"
mkdir -p "$APP_DIR/Contents/MacOS" "$APP_DIR/Contents/Resources"
cp -R "$OUT/publish/." "$APP_DIR/Contents/MacOS/"

cat > "$APP_DIR/Contents/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key><string>PaperTodo</string>
  <key>CFBundleDisplayName</key><string>PaperTodo</string>
  <key>CFBundleIdentifier</key><string>dev.papertodo.mac</string>
  <key>CFBundleVersion</key><string>0.1</string>
  <key>CFBundleShortVersionString</key><string>0.1</string>
  <key>CFBundleExecutable</key><string>PaperTodo</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleInfoDictionaryVersion</key><string>6.0</string>
  <key>LSMinimumSystemVersion</key><string>13.0</string>
  <key>NSHighResolutionCapable</key><true/>
  <!-- Agent app (ADR 0004) lands with the status-bar icon; until then keep a Dock icon
       so the skeleton can be quit normally. Flip LSUIElement to true in that increment. -->
  <key>LSUIElement</key><false/>
</dict>
</plist>
PLIST

chmod +x "$APP_DIR/Contents/MacOS/PaperTodo"
echo "==> $APP_DIR"
