using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using RenyulEngine.Scenes;
using RenyulEngine.Graphics;
using RenyulEngine.Input;
using RenyulEngine.Audio;
using RenyulEngine.Physics;
using RenyulEngine.Assets;

namespace RenyulEngine.Core
{
    public class EngineGame : Game
    {
        private const double FixedDelta = 1.0 / 60.0; // 60 Hz physics
        private double _accumulator = 0.0;

        private readonly GraphicsDeviceManager _gdm;
        private Renderer2D _renderer = null!; // set in Initialize()

        public EngineGame(int width = 1280, int height = 720)
        {
            _gdm = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = width,
                PreferredBackBufferHeight = height,
                SynchronizeWithVerticalRetrace = true
            };

            IsMouseVisible = true;
            Content.RootDirectory = "Content"; // MonoGame content pipeline output
        }

        protected override void Initialize()
        {
            // Register core services in a single place (all must implement IService)
            ServiceLocator.Register(new Time());

            // InputSystem
            var inputSystem = new InputSystem();
            inputSystem.RegisterDevice(new KeyboardDevice());
            inputSystem.RegisterDevice(new MouseDevice());
            ServiceLocator.Register(inputSystem); // InputSystem must implement IService

            // Physics
            var physics = new PhysicsWorld();
            ServiceLocator.Register(physics); // PhysicsWorld implements IService

            // Renderer (must implement IService)
            _renderer = new Renderer2D(GraphicsDevice, Content);
            ServiceLocator.Register(_renderer);

            // Initialize AssetManager (static) so other systems can load resources
            AssetManager.Init(Content, GraphicsDevice);

            // Scene system
            SceneManager.Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Load global resources here if needed (optional).
        }

        protected override void Update(GameTime gameTime)
        {
            // Optional: pause updates when window not active
            if (!IsActive) return;

            // Update time service
            var time = ServiceLocator.Get<Time>();
            time.Update(gameTime);

            // Ensure Start hooks for current scene run once
            SceneManager.EnsureSceneStarted();

            // Process input (InputSystem must be registered)
            ServiceLocator.Get<InputSystem>().Update();

            // Variable-step updates
            SceneManager.Current?.Update();

            // Fixed-step physics loop (accumulator pattern)
_accumulator += gameTime.ElapsedGameTime.TotalSeconds;
time.FixedDelta = FixedDelta;

int maxSteps = 5; // avoid spiral of death
int steps = 0;
while (_accumulator >= FixedDelta && steps < maxSteps)
{
    SceneManager.Current?.FixedUpdate();
    ServiceLocator.Get<PhysicsWorld>().Step(FixedDelta);
    _accumulator -= FixedDelta;
    steps++;
}


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _renderer.Begin();
            SceneManager.Current?.Render(_renderer);
            _renderer.End();

            base.Draw(gameTime);
        }
    }
}
