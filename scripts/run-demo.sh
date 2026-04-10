#!/usr/bin/env bash
set -euo pipefail

CFG=Debug
if [ "${1:-}" = "--release" ] || [ "${1:-}" = "-r" ]; then
  CFG=Release
fi

echo "Building solution (configuration: $CFG)..."
dotnet build MainEngine/RenbokoWorkspace.sln -c $CFG

echo "Launching DemoGame..."
dotnet run --project DemoGame/DemoGame.csproj -c $CFG
