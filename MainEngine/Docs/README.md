<div align="center">
    <img src="../RYENG.png" alt="RYENG" width="320"/>
</div>

# Renboko Game Engine — Documentation

This document provides a concise reference for building, running, and extending the Renboko engine. It targets contributors and developers who need practical instructions and pointers to the code.

## Quick build

From the repository root:

```powershell
dotnet restore
dotnet build RenbokoWorkspace.sln
```

## Run the demo

```powershell
dotnet run --project DemoGame\DemoGame.csproj
```

## Content pipeline

Fonts and some assets are normally consumed as compiled MonoGame `.xnb` files. Use the MonoGame Content Builder (MGCB) or the MGCB Editor to convert `.spritefont` into `.xnb`. Place generated files under `DemoGame/Content/` so the runtime can load them via `ContentManager`.

If you prefer not to run the content pipeline, the engine's `AssetManager` includes a filesystem fallback for textures. Fonts, however, are best supplied as `.xnb`.

## Troubleshooting — common issues

- Missing texture at runtime → verify file path and that the texture exists under `DemoGame/Content/` or the repository root. Use the debug log `renboko_debug.log` for diagnostics.
- UI text missing → confirm `DefaultFont.xnb` is present in `DemoGame/Content/` or run MGCB to build the font from `Content/Default.spritefont`.
- NullReferenceException in scenes → ensure services are registered before scenes use them (register `Renderer2D`, `InputSystem`, then `AssetManager.Init`).
- Build/restore failing in CI on Windows due to path length → set `NUGET_PACKAGES` to a short path in the workflow.

## Examples and pointers

- Core systems live under `MainEngine/Engine/`.
- Example scenes and the demo live in `DemoGame/Scenes/`.
- Use `ServiceLocator` to access registered services. Use `AssetManager.AcquireTexture` and `AcquireSound` for stable, cached loads.

### Scene example

```csharp
public class MyScene : Scene {
    protected override void Start() { /* initialize */ }
    public override void Update() { /* input & logic */ }
    public override void Render(Renderer2D renderer) { /* draw */ }
}
```

## Contributing

- Follow the existing code style. Keep changes small and focused.
- Add unit tests where practical. If adding assets, include instructions to rebuild MGCB content.

## License

MIT — see LICENSE file.
