<div align="center">
    <img src="RYENG.png" alt="RYENG" width="320"/>
</div>



# Renyul Game Engine

>A hobbyist 2D game engine built on MonoGame, with a simple demo game and a focus on learning, tinkering, and having fun.

---

## What is this?

Renyul Game Engine is a modular, service-based 2D game engine for .NET 8 and MonoGame 3.8.4. It comes with a small demo game to show how things work. The code and comments are written in a casual, hobbyist style—so don't expect enterprise stuff here!

---

## Features
- Scene system with easy switching
- Service locator for global systems (input, rendering, physics, audio, etc.)
- 2D renderer with camera support
- Physics (basic rigidbodies, box/circle colliders, collisions)
- Asset manager for textures, audio, etc.
- Retained-mode UI (buttons, labels, sliders, etc.)
- Simple audio system with groups and volume control
- Example demo game included

---

## Building & Running

1. **Build everything:**
    ```sh
    dotnet build RenyulWorkspace.sln
    ```
2. **Run the demo game:**
    ```sh
    dotnet run --project DemoGame/DemoGame.csproj
    ```

---

## Documentation
- See `Docs/README.md` for full engine docs, API, and examples.
- See `Docs/DemoGame.md` for a walkthrough of the demo game.

---

## Troubleshooting
- If something breaks, check the troubleshooting sections in the docs. Most issues are missing assets, unregistered services, or typos.
- If you fix a weird bug, add it to the docs for the next person!

---

## License

This project is licensed under the GNU General Public License v3.0 (GPLv3).

```
Renyul Game Engine
Copyright (C) 2025  TawhidBinOmar

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
```
