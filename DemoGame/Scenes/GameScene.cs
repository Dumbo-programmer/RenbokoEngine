using RenyulEngine.Scenes;
using RenyulEngine.Physics;
using RenyulEngine.Graphics;
using RenyulEngine.Core;
using Microsoft.Xna.Framework;

namespace DemoGame
{
    public class GameScene : Scene
    {
    private Rigidbody2D? player;
    private Sprite? playerSprite;
    private Sprite? coinSprite;
    private Vector2 playerPosition = new Vector2(100, 100);
    private Vector2 coinPosition = new Vector2(400, 300);

        public GameScene() {}

        protected override void Start()
        {
            // Player physics body
            player = new Rigidbody2D(new BoxCollider2D(new Vector2(32, 32)));
            player.Position = playerPosition;

            // Player sprite
            var playerTex = RenyulEngine.Assets.AssetManager.AcquireTexture("player.png");
            playerSprite = new Sprite(playerTex);

            // Coin sprite
            var coinTex = RenyulEngine.Assets.AssetManager.AcquireTexture("coin.png");
            coinSprite = new Sprite(coinTex);
        }

        public override void Update()
        {
            // Player input

            var input = ServiceLocator.Get<RenyulEngine.Input.InputSystem>();
            var keyboard = input.GetDevice<RenyulEngine.Input.KeyboardDevice>();
            var time = ServiceLocator.Get<RenyulEngine.Core.Time>();
            if (keyboard != null && time != null)
            {
                if (keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.Right))
                    playerPosition.X += 200 * time.DeltaTime;
                if (keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.Left))
                    playerPosition.X -= 200 * time.DeltaTime;
            }

            // Simple collision with coin
            if (Vector2.Distance(playerPosition, coinPosition) < 32)
            {
                // "Collect" coin
                coinPosition = new Vector2(-100, -100); // hide
            }
        }

        public override void Render(Renderer2D renderer)
        {
            renderer.Begin();
            if (playerSprite != null)
                playerSprite.Draw(renderer, playerPosition);
            if (coinSprite != null)
                coinSprite.Draw(renderer, coinPosition);
            renderer.End();
        }
    }
}
