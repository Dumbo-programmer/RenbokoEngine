using System.Collections.Generic;
using RenyulEngine.Scenes;
using RenyulEngine.UI;
using RenyulEngine.Core;

namespace DemoGame
{
    public class MainMenuScene : Scene
    {
        private UIButton? startButton;
        private readonly List<UIElement> _uiElements = new();

        protected override void Start()
        {
            startButton = new UIButton();
            startButton.Text = "Start Game";
            startButton.OnClick += () => SceneManager.Load(new GameScene());
            _uiElements.Add(startButton);
        }

        public override void Update()
        {
            foreach (var ui in _uiElements)
                ui.Update();
        }

        public override void Render(RenyulEngine.Graphics.Renderer2D renderer)
        {
            // You may want to load a font from renderer or AssetManager
            var font = renderer.LoadFont("DefaultFont"); // Replace with your font asset name
            foreach (var ui in _uiElements)
                ui.Draw(renderer, font);
        }
    }
}
