<#
Builds the solution and runs the DemoGame project in showcase mode (sets RENBE_SHOWCASE=1).
Usage:
  .\run-showcase.ps1          # runs in Debug
  .\run-showcase.ps1 -Release # runs in Release
#>
param(
    [switch]$Release
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$cfg = if ($Release) { 'Release' } else { 'Debug' }

Write-Host "Building solution (configuration: $cfg)..."
dotnet build MainEngine\RenbokoWorkspace.sln -c $cfg
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed (exit $LASTEXITCODE)"; exit $LASTEXITCODE }

Write-Host "Launching DemoGame in Showcase mode..."
$env:RENBE_SHOWCASE = '1'
dotnet run --project DemoGame\DemoGame.csproj -c $cfg
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet run failed (exit $LASTEXITCODE)"; exit $LASTEXITCODE }