#!/usr/bin/env bash
set -euo pipefail

CFG=Debug
if [ "${1:-}" = "--release" ] || [ "${1:-}" = "-r" ]; then
  CFG=Release
fi

OUT_DIR="artifacts/demo/$CFG"

echo "Publishing DemoGame ($CFG)..."
dotnet publish DemoGame/DemoGame.csproj -c "$CFG" -o "$OUT_DIR"

echo "Package ready at: $OUT_DIR"
