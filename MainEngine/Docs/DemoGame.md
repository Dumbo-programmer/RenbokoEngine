<div align="center">
    <img src="../RYENG.png" alt="RYENG" width="320"/>
</div>

# DemoGame — Reference

This document describes the included `DemoGame` project. The demo is a minimal example showing how scenes, UI, input, asset loading, and simple physics integrate with the engine.

## Build & run

```powershell
dotnet restore
dotnet build RenbokoWorkspace.sln
dotnet run --project DemoGame\DemoGame.csproj
```

Alternatively use the provided scripts:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\run-demo.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\run-showcase.ps1
```

## Controls

- Mouse: click UI buttons on the menu
- Left / Right arrows: move the player horizontally in the game scene

## Project layout

```
DemoGame/
├─ GameApp.cs      # EngineGame subclass and startup logic
├─ Program.cs      # dotnet entry point
└─ Scenes/
   ├─ MainMenuScene.cs
   └─ GameScene.cs
```

## Scenes

MainMenuScene
- Creates UI elements and handles navigation. The start button loads `GameScene`.

GameScene
- Demonstrates sprite rendering, basic input handling, and a simple collect mechanic.

## Key implementation notes

- Asset loading: use `AssetManager.AcquireTexture("name.png")`. Textures have a filesystem fallback; fonts are expected as `.xnb` built with MGCB.
- Debugging: runtime messages are appended to `renboko_debug.log` at the repository root.
- Service ordering: ensure `Renderer2D` and `AssetManager.Init` are registered before scenes attempt to acquire assets.

## Troubleshooting

- Missing textures or audio → confirm asset paths and that the files are present under `DemoGame/Content/` or repository root.
- UI text missing → generate `DefaultFont.xnb` with MGCB and place it in `DemoGame/Content/`.
- Game window freezes or build fails → make sure no running DemoGame process is locking the build outputs; kill running processes before rebuilding.

## Extending the demo

- Add scenes by inheriting from `Scene` and calling `SceneManager.Load(new YourScene())`.
- To add a new texture or sprite, place the file under `DemoGame/Content/` and use `AssetManager.AcquireTexture("your.png")`.

For runtime examples and full API usage, see the top-level documentation and the `MainEngine/Docs/README.md` file.
