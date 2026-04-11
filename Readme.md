<div align="center">
    <img src="RY.png" alt="Renboko" width="320"/>
</div>

# Renboko Game Engine

Renboko is a 2D game engine built on .NET 8 and MonoGame 3.8.4. The repository includes the engine, a playable demo, scripts for local workflows, and CI automation.

## Repository layout

- `MainEngine/`: engine project and core systems
- `DemoGame/`: runnable sample game
- `Tests/`: unit tests for core logic
- `docs/`: static docs site
- `scripts/`: build/run/publish helpers

## Features

- Scene-based gameplay flow
- Service locator for core systems
- 2D renderer with camera transform
- Input, audio, and basic physics primitives
- Asset manager with caching and fallback loading
- Retained-mode UI controls
- Endless-runner demo mode in `DemoGame`

## Build, run, test

From repository root:

```powershell
dotnet restore
dotnet build MainEngine\RenbokoWorkspace.sln
dotnet run --project DemoGame\DemoGame.csproj
dotnet test Tests\RenbokoEngine.Tests\RenbokoEngine.Tests.csproj
```

## Scripts

- Build and run demo: `scripts/build-and-run.ps1`
- Run demo directly: `scripts/run-demo.ps1`
- Run showcase mode: `scripts/run-showcase.ps1`
- Publish package: `scripts/publish-demo.ps1`

POSIX equivalents are available as `.sh` scripts.

## Documentation

- Engine docs: `MainEngine/Docs/README.md`
- Demo docs: `MainEngine/Docs/DemoGame.md`
- Web docs: `docs/index.html`
- Release process: `RELEASE.md`
- Changelog: `CHANGELOG.md`

## License

GPL v3.0.
