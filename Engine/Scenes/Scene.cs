using RenyulEngine.Graphics;

namespace RenyulEngine.Scenes
{
    public abstract class Scene
    {
        private bool _started;
        public bool Started => _started;

        /// <summary>
        /// Called once when the scene becomes active (after Load).
        /// Use this to instantiate runtime objects and request assets.
        /// </summary>
        protected virtual void Start() {}

        /// <summary>
        /// Called every variable-step frame.
        /// </summary>
        public virtual void Update() {}

        /// <summary>
        /// Called every fixed-step physics tick.
        /// </summary>
        public virtual void FixedUpdate() {}

        /// <summary>
        /// Render your scene using the provided renderer.
        /// </summary>
        public virtual void Render(Renderer2D renderer) {}

        /// <summary>
        /// Called when the scene is unloaded.
        /// </summary>
        public virtual void OnUnload() {}

        /// <summary>
        /// Internal startup — ensures Start() is called once.
        /// </summary>
        internal void StartInternal()
        {
            if (_started) return;
            _started = true;
            Start();
        }
    }
}
