<#
Builds the solution and runs the DemoGame project.
Usage:
  .\run-demo.ps1          # runs in Debug
  .\run-demo.ps1 -Release # runs in Release
#>
param(
    [switch]$Release
)

$cfg = if ($Release) { 'Release' } else { 'Debug' }

Write-Host "Building solution (configuration: $cfg)..."
dotnet build MainEngine\RenbokoWorkspace.sln -c $cfg
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed (exit $LASTEXITCODE)"; exit $LASTEXITCODE }

Write-Host "Launching DemoGame..."
dotnet run --project DemoGame\DemoGame.csproj -c $cfg