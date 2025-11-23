using RenbokoEngine.Core;
using RenbokoEngine.Scenes;
using System;   
namespace DemoGame
{
    public class GameApp : EngineGame
    {
        protected override void Initialize()
        {
            base.Initialize();

            // Register your starting scene
            SceneManager.Load(new MainMenuScene());
        }
    }
}
