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

            // If the environment variable RENBE_SHOWCASE is set (1/true), start the ShowcaseScene.
            // Otherwise, load the normal main menu.
            var showcase = Environment.GetEnvironmentVariable("RENBE_SHOWCASE");
            if (!string.IsNullOrEmpty(showcase) && (showcase == "1" || showcase.Equals("true", StringComparison.OrdinalIgnoreCase)))
            {
                SceneManager.Load(new ShowcaseScene());
            }
            else
            {
                // Register your starting scene
                SceneManager.Load(new MainMenuScene());
            }
        }
    }
}
