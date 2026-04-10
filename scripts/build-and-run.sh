#!/usr/bin/env bash
set -euo pipefail

CFG=Debug
if [ "${1:-}" = "--release" ] || [ "${1:-}" = "-r" ]; then
  CFG=Release
fi

echo "Building solution (configuration: $CFG)..."
dotnet build MainEngine/RenbokoWorkspace.sln -c $CFG

EXE="./DemoGame/bin/$CFG/net8.0/DemoGame"
DLL="./DemoGame/bin/$CFG/net8.0/DemoGame.dll"

if [ -f "$EXE" ]; then
  echo "Launching executable: $EXE"
  "$EXE"
elif [ -f "$DLL" ]; then
  echo "Launching via dotnet: $DLL"
  dotnet "$DLL"
else
  echo "Falling back to dotnet run"
  dotnet run --project DemoGame/DemoGame.csproj -c $CFG
fi
