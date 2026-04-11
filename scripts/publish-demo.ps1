<#
Publishes DemoGame and copies release assets to artifacts/demo.
Usage:
  .\publish-demo.ps1          # Debug
  .\publish-demo.ps1 -Release # Release
#>
param(
    [switch]$Release
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$cfg = if ($Release) { 'Release' } else { 'Debug' }
$outDir = Join-Path -Path "artifacts\demo" -ChildPath $cfg

Write-Host "Publishing DemoGame ($cfg)..."
dotnet publish DemoGame\DemoGame.csproj -c $cfg -o $outDir
if ($LASTEXITCODE -ne 0) { Write-Error "Publish failed (exit $LASTEXITCODE)"; exit $LASTEXITCODE }

Write-Host "Package ready at: $outDir"
