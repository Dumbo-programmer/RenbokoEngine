using System.Collections.Generic;
using RenyulEngine.Scenes;
using RenyulEngine.UI;
using RenyulEngine.Core;

namespace DemoGame
{
    public class MainMenuScene : Scene
    {
    // No UI elements or assets needed for zero-asset demo

        protected override void Start()
        {
            // No asset loading or UI setup
        }

        public override void Update()
        {
            // No UI to update
        }

        public override void Render(RenyulEngine.Graphics.Renderer2D renderer)
        {
            // Just clear the screen with a color (done in EngineGame), nothing else to render
        }
    }
}
