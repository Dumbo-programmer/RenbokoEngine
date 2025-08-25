using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics; // For System.Numerics.Microsoft.Xna.Framework.Vector2 in DTOs
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using RenyulEngine.Graphics;
using RenyulEngine.Physics;
using RenyulEngine.Core;
using RenyulEngine.Assets;
using RenyulEngine.Audio;

namespace RenyulEngine.Scenes
{
    // This thing loads and saves scenes as JSON. It's pretty simple: just a list of entities, each with a transform and maybe a sprite, rigidbody, or collider.
    // If you want to make your own scene format, you can totally change this. I just wanted something that works for small games.
    public static class SceneLoader
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        // DTOs for serialization
        public class SceneData
        {
            public string Name { get; set; } = "Untitled";
            public List<EntityData> Entities { get; set; } = new();
        }

        public class EntityData
        {
            public string Name { get; set; } = "Entity";
            public Microsoft.Xna.Framework.Vector2 Position { get; set; } = new Microsoft.Xna.Framework.Vector2(0,0);
            public float Rotation { get; set; } = 0f;
            public Microsoft.Xna.Framework.Vector2 Scale { get; set; } = new Microsoft.Xna.Framework.Vector2(1,1);

            // Optional components:
            public SpriteComponentData? Sprite { get; set; }
            public RigidbodyComponentData? Rigidbody { get; set; }
            public ColliderComponentData? Collider { get; set; }
            public AudioComponentData? Audio { get; set; }
        }

        public class SpriteComponentData
        {
            public string TexturePath { get; set; } = "";
            public Microsoft.Xna.Framework.Vector2 Origin { get; set; } = new Microsoft.Xna.Framework.Vector2(0,0);
            public float Layer { get; set; } = 0f;
        }

        public class RigidbodyComponentData
        {
            public float Mass { get; set; } = 1f;
            public bool IsStatic { get; set; } = false;
            public Microsoft.Xna.Framework.Vector2 InitialVelocity { get; set; } = new Microsoft.Xna.Framework.Vector2(0,0);
        }

        public class ColliderComponentData
        {
            public string Type { get; set; } = "Box"; // "Box" or "Circle"
            public Microsoft.Xna.Framework.Vector2 Size { get; set; } = new Microsoft.Xna.Framework.Vector2(32,32); // for Box
            public float Radius { get; set; } = 16f; // for Circle
            public Microsoft.Xna.Framework.Vector2 Offset { get; set; } = new Microsoft.Xna.Framework.Vector2(0,0);
        }

        public class AudioComponentData
        {
            public string ClipPath { get; set; } = "";
            public string Group { get; set; } = "Default";
            public float Volume { get; set; } = 1f;
            public bool Loop { get; set; } = false;
        }

        /// <summary>
        /// Save a Scene to a JSON file. Only scenes that are RuntimeScene (produced by this loader) are supported.
        /// </summary>
        public static async Task SaveToFileAsync(Scene scene, string filePath)
        {
            if (scene is not RuntimeScene runtime)
                throw new InvalidOperationException("Only RuntimeScene instances created by SceneLoader can be saved with full fidelity.");

            var data = runtime.ToSceneData();
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            await File.WriteAllTextAsync(filePath, json);
        }

        /// <summary>
        /// Loads a scene from a JSON file, returns a Scene instance (RuntimeScene) ready to be made current.
        /// This will not call Scene.Start() — SceneManager.ApplyPending will run StartInternal as part of switching.
        /// </summary>
        public static async Task<Scene> LoadFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("Scene file not found", filePath);
            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<SceneData>(json, _jsonOptions) ?? new SceneData();
            return await BuildRuntimeSceneFromDataAsync(data);
        }

        /// <summary>
        /// Convert SceneData -> RuntimeScene (this is async because it may load assets).
        /// </summary>
        private static async Task<Scene> BuildRuntimeSceneFromDataAsync(SceneData data)
        {
            // Create a RuntimeScene which will create entities in Start() after assets are loaded here
            var runtime = new RuntimeScene(data.Name, data);
            // Preload simple assets if desired (textures, audio). We'll use renderer/content via ServiceLocator.
            // Because Asset loading is synchronous in MonoGame ContentManager, we don't do heavy async here.
            await Task.Yield();
            return runtime;
        }

        /// <summary>
        /// Concrete Scene that knows how to instantiate EntityData into runtime objects.
        /// </summary>
        private class RuntimeScene : Scene
        {
            private readonly SceneData _data;
            private readonly List<SceneEntity> _entities = new();

            public RuntimeScene(string name, SceneData data)
            {
                _data = data;
            }

            /// <summary>
            /// Build runtime objects (load textures, create sprites, physics bodies, audio sources).
            /// </summary>
            protected override void Start()
            {
                var renderer = ServiceLocator.Get<Renderer2D>();
                var physics = ServiceLocator.Get<PhysicsWorld>();
                var assetMgr = AssetManagerHelper.GetContent(); // wrapper to get ContentManager if you have one

                foreach (var ed in _data.Entities)
                {
                    var e = new SceneEntity { Name = ed.Name };
                    // Transform
                    e.Position = new Microsoft.Xna.Framework.Vector2(ed.Position.X, ed.Position.Y);
                    e.Rotation = ed.Rotation;
                    e.Scale = new Microsoft.Xna.Framework.Vector2(ed.Scale.X, ed.Scale.Y);
//sprite
if (ed.Sprite != null && !string.IsNullOrWhiteSpace(ed.Sprite.TexturePath))
{
    Texture2D tex;
    try
    {
        tex = renderer.LoadTexture(ed.Sprite.TexturePath);
    }
    catch
    {
        tex = AssetManagerHelper.LoadTexture(ed.Sprite.TexturePath);
    }

    e.Sprite = new Sprite(tex)
    {
        Origin = ed.Sprite.Origin,
        Layer = ed.Sprite.Layer
    };
    e.SpriteTexturePath = ed.Sprite.TexturePath;
}


                    // Rigidbody + Collider
                    if (ed.Rigidbody != null)
                    {
                        Collider2D collider = null!;
                        if (ed.Collider != null)
                        {
                            if (string.Equals(ed.Collider.Type, "Circle", StringComparison.OrdinalIgnoreCase))
                            {
                                var circle = new CircleCollider2D(ed.Collider.Radius);
                                collider = circle;
                            }
                            else // box
                            {
                                var box = new BoxCollider2D(new Microsoft.Xna.Framework.Vector2(ed.Collider.Size.X, ed.Collider.Size.Y));
                                collider = box;
                            }
                        }
                        else
                        {
                            // default small box
                            collider = new BoxCollider2D(new Microsoft.Xna.Framework.Vector2(32, 32));
                        }

                        var rb = new Rigidbody2D(collider)
                        {
                            Mass = ed.Rigidbody.Mass,
                            IsStatic = ed.Rigidbody.IsStatic,
                            Velocity = new Microsoft.Xna.Framework.Vector2(ed.Rigidbody.InitialVelocity.X, ed.Rigidbody.InitialVelocity.Y)
                        };

                        rb.Position = new Microsoft.Xna.Framework.Vector2(ed.Position.X, ed.Position.Y);
                        physics.AddBody(rb);

                        e.Rigidbody = rb;
                    }

                    // Audio
if (ed.Audio != null && !string.IsNullOrWhiteSpace(ed.Audio.ClipPath))
{
    try
    {
        var sound = AssetManagerHelper.LoadSoundEffect(ed.Audio.ClipPath);
        var src = new AudioSource(sound, ed.Audio.Group)
        {
            Volume = ed.Audio.Volume,
            Loop = ed.Audio.Loop
        };
        e.AudioSource = src;
        e.AudioClipPath = ed.Audio.ClipPath;
    }
    catch
    {
        // ignore
    }
}


                    _entities.Add(e);
                }
            }

            public override void Update()
            {
                // Drawables/logic may be kept here — but scene-level update is empty by default.
            }

            public override void Render(Renderer2D renderer)
            {
                foreach (var e in _entities)
                {
                    if (e.Sprite != null)
                    {
                        e.Sprite.Draw(renderer, new Microsoft.Xna.Framework.Vector2(e.Position.X, e.Position.Y));
                    }
                }
            }

            public override void OnUnload()
            {
                // Dispose audio sources and any created resources
                foreach (var e in _entities)
                {
                    e.AudioSource?.Dispose();
                }

                _entities.Clear();
            }

            /// <summary>
            /// Convert runtime scene back to serializable data (for Save).
            /// Only supports entities created from the DTO fields.
            /// </summary>
            internal SceneData ToSceneData()
            {
                var sd = new SceneData { Name = _data.Name };
                foreach (var e in _entities)
                {
                    var ed = new EntityData
                    {
                        Name = e.Name ?? "Entity",
                        Position = new Microsoft.Xna.Framework.Vector2(e.Position.X, e.Position.Y),
                        Rotation = e.Rotation,
                        Scale = new Microsoft.Xna.Framework.Vector2(e.Scale.X, e.Scale.Y)
                    };

                    if (e.Sprite != null)
                    {
                        ed.Sprite = new SpriteComponentData
                        {
                            TexturePath = e.SpriteTexturePath ?? "",
                            Origin = new Microsoft.Xna.Framework.Vector2(e.Sprite.Origin.X, e.Sprite.Origin.Y),
                            Layer = e.Sprite.Layer
                        };
                    }

                    if (e.Rigidbody != null)
                    {
                        ed.Rigidbody = new RigidbodyComponentData
                        {
                            Mass = e.Rigidbody.Mass,
                            IsStatic = e.Rigidbody.IsStatic,
                            InitialVelocity = new Microsoft.Xna.Framework.Vector2(e.Rigidbody.Velocity.X, e.Rigidbody.Velocity.Y)
                        };
                        if (e.Rigidbody.Collider is BoxCollider2D box)
                        {
                            ed.Collider = new ColliderComponentData
                            {
                                Type = "Box",
                                Size = new Microsoft.Xna.Framework.Vector2(box.Size.X, box.Size.Y)
                            };
                        }
                        else if (e.Rigidbody.Collider is CircleCollider2D circle)
                        {
                            ed.Collider = new ColliderComponentData
                            {
                                Type = "Circle",
                                Radius = circle.Radius
                            };
                        }
                    }

                    if (e.AudioSource != null)
                    {
                        ed.Audio = new AudioComponentData
                        {
                            ClipPath = e.AudioClipPath ?? "",
                            Group = e.AudioSource.Group?.Name ?? "Default",
                            Volume = e.AudioSource.Volume,
                            Loop = e.AudioSource.Loop
                        };
                    }

                    sd.Entities.Add(ed);
                }

                return sd;
            }

            // Simple runtime entity container
            private class SceneEntity
            {
                public string? Name;
                public Microsoft.Xna.Framework.Vector2 Position;
                public float Rotation;
                public Microsoft.Xna.Framework.Vector2 Scale = Microsoft.Xna.Framework.Vector2.One;

                public Sprite? Sprite;
                public string? SpriteTexturePath;

                public Rigidbody2D? Rigidbody;

                public AudioSource? AudioSource;
                public string? AudioClipPath;
            }
        }
    }

    // Small helpers for asset loading integration — adapt to your AssetManager/Content pipeline implementation.
    internal static class AssetManagerHelper
    {
        public static Microsoft.Xna.Framework.Content.ContentManager? GetContent()
        {
            // Return ContentManager if you keep a global reference somewhere.
            // For now, we try to get it via Renderer2D's internal Content field using reflection (if present).
            try
            {
                var renderer = ServiceLocator.Get<Renderer2D>();
                var field = typeof(Renderer2D).GetField("_content", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) return (Microsoft.Xna.Framework.Content.ContentManager?)field.GetValue(renderer);
            }
            catch { }
            return null;
        }

        public static Microsoft.Xna.Framework.Graphics.Texture2D LoadTexture(string path)
        {
            var renderer = ServiceLocator.Get<Renderer2D>();
            return renderer.LoadTexture(path);
        }

        public static Microsoft.Xna.Framework.Audio.SoundEffect LoadSoundEffect(string path)
        {
            try
            {
                var field = typeof(Renderer2D).GetField("_content", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var renderer = ServiceLocator.Get<Renderer2D>();
                var content = (Microsoft.Xna.Framework.Content.ContentManager?)field.GetValue(renderer);
                if (content != null)
                {
                    return content.Load<Microsoft.Xna.Framework.Audio.SoundEffect>(path);
                }
            }
            catch { }

            throw new InvalidOperationException("Could not load sound via ContentManager. Hook AssetManagerHelper.LoadSoundEffect to your audio/content loader.");
        }
    }
}
