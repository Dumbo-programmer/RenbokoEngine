<#
Builds the solution and runs the DemoGame project.
Usage:
  .\build-and-run.ps1          # runs in Debug
  .\build-and-run.ps1 -Release # runs in Release
#>
param(
    [switch]$Release
)

$cfg = if ($Release) { 'Release' } else { 'Debug' }

Write-Host "Building solution (configuration: $cfg)..."
dotnet build MainEngine\RenbokoWorkspace.sln -c $cfg
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed (exit $LASTEXITCODE)"; exit $LASTEXITCODE }

$exePath = Join-Path -Path "DemoGame\bin\$cfg\net8.0" -ChildPath "DemoGame.exe"
if (Test-Path $exePath) {
    Write-Host "Launching executable: $exePath"
    & $exePath
} else {
    Write-Host "Executable not found, falling back to dotnet run"
    dotnet run --project DemoGame\DemoGame.csproj -c $cfg
}
