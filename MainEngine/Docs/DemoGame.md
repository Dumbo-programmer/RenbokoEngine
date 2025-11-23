<div align="center">
    <img src="../RYENG.png" alt="RYENG" width="320"/>
</div>

# DemoGame for Renboko Game Engine

## Building and Running the Game

1. **Build the solution:**
    ```sh
    dotnet build RenbokoWorkspace.sln
    ```
2. **Run the demo game:**
    ```sh
    dotnet run --project DemoGame/DemoGame.csproj
    ```
3. **Controls:**
    - Use the mouse to click the "Start Game" button.
    - Use the left/right arrow keys to move the player in the game scene.

---

This document explains the structure and code of the included `DemoGame` project, which serves as a practical example of how to use the Renboko Game Engine for a simple 2D game.

---

## Table of Contents
- [Overview](#overview)
- [Project Structure](#project-structure)
- [GameApp Entry Point](#gameapp-entry-point)
- [MainMenuScene](#mainmenuscene)
- [GameScene](#gamescene)
- [Key Features Demonstrated](#key-features-demonstrated)
- [How to Extend](#how-to-extend)
- [Running the Demo](#running-the-demo)

---

## Overview
The DemoGame is a minimal but complete game built with the Renboko Game Engine. It demonstrates scene management, input handling, sprite rendering, UI, and basic collision logic.

---

## Project Structure
```
DemoGame/
├── GameApp.cs         # Main game class, sets up the starting scene
├── Program.cs         # Entry point
└── Scenes/
    ├── MainMenuScene.cs  # Main menu with a start button
    └── GameScene.cs      # Simple gameplay scene (player, coin)
```


## Troubleshooting & Common Bug Fixes

Some stuff that tripped me up while making the demo (maybe it'll help you too):

- **Game doesn't start or crashes:**
    - Check if you built the solution and all assets are in the right place. Missing files are the usual culprit.
- **Player doesn't move:**
    - Make sure the input system is set up and you're actually checking for key presses in your update loop.
- **UI buttons don't work:**
    - Double-check your button is in the update and draw lists, and that the mouse is over the button when you click.
- **Sprites are invisible:**
    - Make sure the texture is loaded and you're not drawing it off-screen (camera position matters!).
- **Weird physics/collisions:**
    - Check collider sizes and positions. Sometimes a typo in the numbers makes things go wild.
- **Build errors about missing MonoGame or .NET:**
    - Make sure you have the right versions installed. If you just installed them, restart your editor/terminal.
- **Other random bugs:**
    - Most of the time it's a typo, missing asset, or forgot to add something to a list. Read the error message, it'll usually tell you what's up.

If you fix a weird bug, add it here for the next person!


## GameApp Entry Point
`GameApp` inherits from `EngineGame` and sets the initial scene:
```csharp
public class GameApp : EngineGame {
    protected override void Initialize() {
        base.Initialize();
        SceneManager.Load(new MainMenuScene());
    }
}
```

---

## MainMenuScene
- Inherits from `Scene`.
- Creates a `UIButton` labeled "Start Game".
- When clicked, loads the `GameScene`.
- UI elements are managed in a list and updated/drawn each frame.

**Example:**
```csharp
protected override void Start() {
    startButton = new UIButton();
    startButton.Text = "Start Game";
    startButton.OnClick += () => SceneManager.Load(new GameScene());
    _uiElements.Add(startButton);
}
```

---

## GameScene
- Inherits from `Scene`.
- Demonstrates sprite rendering, input, and simple collision.
- Player can move left/right with arrow keys.
- If the player collides with the coin, the coin is "collected" (hidden).

**Example:**
```csharp
protected override void Start() {
    player = new Rigidbody2D(new BoxCollider2D(new Vector2(32, 32)));
    player.Position = new Vector2(100, 100);
    var playerTex = AssetManager.AcquireTexture("player.png");
    playerSprite = new Sprite(playerTex);
    var coinTex = AssetManager.AcquireTexture("coin.png");
    coinSprite = new Sprite(coinTex);
}

public override void Update() {
    var input = ServiceLocator.Get<InputSystem>();
    var keyboard = input.GetDevice<KeyboardDevice>();
    var time = ServiceLocator.Get<Time>();
    if (keyboard != null && time != null) {
        if (keyboard.GetKey(Keys.Right)) playerPosition.X += 200 * time.DeltaTime;
        if (keyboard.GetKey(Keys.Left)) playerPosition.X -= 200 * time.DeltaTime;
    }
    if (Vector2.Distance(playerPosition, coinPosition) < 32) {
        coinPosition = new Vector2(-100, -100); // hide
    }
}
```

---

## Key Features Demonstrated
- Scene switching (`SceneManager.Load`)
- UI button with click event
- Sprite rendering
- Keyboard input
- Simple collision logic
- Asset loading via `AssetManager`

---

## How to Extend
- Add more scenes (e.g., game over, settings) by inheriting from `Scene`.
- Add more UI elements (labels, sliders, dropdowns) to your scenes.
- Add more gameplay logic (physics, scoring, enemies) in your `Update`/`FixedUpdate` methods.
- Use the engine’s audio system to add sound effects.

---

## Running the Demo
1. Build the solution: `dotnet build RenbokoWorkspace.sln`
2. Run the demo: `dotnet run --project DemoGame/DemoGame.csproj`
3. Use the UI and keyboard to interact with the demo.

---

For more details, see the main [Renboko Game Engine Documentation](./README.md).
