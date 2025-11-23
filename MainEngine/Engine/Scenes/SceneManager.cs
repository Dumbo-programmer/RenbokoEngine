using System;
using System.IO;
using System.Threading.Tasks;

namespace RenbokoEngine.Scenes
{
    public static class SceneManager
    {
        public static Scene? Current { get; private set; }
        private static Scene? _pending;

        public static void Initialize() { }

        public static void EnsureSceneStarted() => Current?.StartInternal();

        /// <summary>
        /// Immediately switch to scene instance (sync).
        /// </summary>
        public static void Load(Scene scene)
        {
            _pending = scene;
            ApplyPending();
        }

        /// <summary>
        /// Load a scene produced by an async factory (e.g. building assets).
        /// </summary>
        public static async Task LoadAsync(Func<Task<Scene>> build)
        {
            var scene = await build();
            _pending = scene;
            ApplyPending();
        }

        /// <summary>
        /// Load a serialized scene from disk asynchronously (.json).
        /// Uses SceneLoader.LoadFromFileAsync to produce a Scene instance.
        /// </summary>
        public static async Task LoadFromFileAsync(string filePath)
        {
            var scene = await SceneLoader.LoadFromFileAsync(filePath);
            _pending = scene;
            ApplyPending();
        }

        /// <summary>
        /// Save the given scene to disk asynchronously.
        /// </summary>
        public static async Task SaveToFileAsync(Scene scene, string filePath)
        {
            await SceneLoader.SaveToFileAsync(scene, filePath);
        }

        private static void ApplyPending()
        {
            if (_pending == null) return;
            Current?.OnUnload();
            Current = _pending;
            _pending = null;
        }
    }
}
