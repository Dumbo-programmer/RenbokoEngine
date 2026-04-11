using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace RenbokoEngine.Assets
{
    /// <summary>
    /// Central asset manager for Renboko Engine.
    /// - Initialize with a ContentManager and GraphicsDevice (or via Renderer2D).
    /// - Caches assets and supports Acquire/Release (reference counting).
    /// - Provides both sync and async load helpers. Note: MonoGame ContentManager.Load is generally synchronous and not thread-safe.
    /// </summary>
    public static class AssetManager
    {
        private static ContentManager? _content;
        private static GraphicsDevice? _graphicsDevice;

        // caches + ref counts
        private static readonly Dictionary<string, Texture2D> _textures = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> _textureRef = new(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, SpriteFont> _fonts = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> _fontRef = new(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, SoundEffect> _sfx = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> _sfxRef = new(StringComparer.OrdinalIgnoreCase);

        // concurrent cache for async-from-stream loads
        private static readonly ConcurrentDictionary<string, Task<Texture2D>?> _asyncTextureLoads = new();

        /// <summary>
        /// Initialize AssetManager with MonoGame ContentManager and GraphicsDevice.
        /// Call this during Engine initialization.
        /// </summary>
        public static void Init(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        }

        #region Textures

        private const string BuiltinTexturePrefix = "__builtin";

        /// <summary>
        /// Synchronous Acquire (load or increase ref count) a texture by asset name.
        /// The assetName can be a content pipeline id (e.g. "textures/crate") or a file path (absolute/relative).
        /// </summary>
        public static Texture2D AcquireTexture(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            if (_textures.TryGetValue(path, out var existing))
            {
                _textureRef[path]++;
                return existing;
            }

            // Try content first (main thread)
            if (_content != null)
            {
                try
                {
                    var tex = _content.Load<Texture2D>(path);
                    _textures[path] = tex;
                    _textureRef[path] = 1;
                    return tex;
                }
                catch { /* fall back to file-based stream */ }
            }

            // Fallback to file-based load using FromStream (requires GraphicsDevice)
            var fromFile = LoadTextureFromFile(path);
            if (fromFile == null) throw new FileNotFoundException($"Texture not found: {path}");

            _textures[path] = fromFile!;
            _textureRef[path] = 1;
            return fromFile!;
        }

        /// <summary>
        /// Async texture loader that uses a file stream fallback.
        /// Returns an in-flight task if already loading.
        /// </summary>
        public static Task<Texture2D> AcquireTextureAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            // If texture already loaded synchronously
            if (_textures.TryGetValue(path, out var existing))
            {
                _textureRef[path]++;
                return Task.FromResult(existing);
            }

            // If an async load is already in progress, return that Task
            if (_asyncTextureLoads.TryGetValue(path, out var inFlight) && inFlight != null) return inFlight;

            // Create a task that will try to load using file fallback (Texture2D.FromStream).
            var t = Task.Run(() =>
            {
                // Try content first (not safe on background thread for many MG builds) - so prefer file fallback
                var tex = LoadTextureFromFile(path);
                if (tex == null)
                {
                    // If no file exists, as last resort attempt a synchronous content load on the calling thread
                    throw new FileNotFoundException($"Could not find texture for async load: {path}");
                }

                lock (_textures)
                {
                    if (!_textures.ContainsKey(path))
                    {
                        _textures[path] = tex;
                        _textureRef[path] = 1;
                        return tex;
                    }
                    else
                    {
                        // another thread inserted same texture
                        var existing = _textures[path];
                        tex.Dispose();
                        _textureRef[path]++;
                        return existing;
                    }
                }
            });

            // store the in-flight task
            _asyncTextureLoads[path] = t;
            // remove the entry when done (continuation)
            t.ContinueWith(_ => { _asyncTextureLoads.TryRemove(path, out Task<Texture2D>? _removed); });
            return t;
        }

        /// <summary>
        /// Release a previously acquired texture. When ref count hits 0, texture is disposed and removed from cache.
        /// </summary>
        public static void ReleaseTexture(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName)) return;
            if (!_textures.ContainsKey(assetName)) return;

            _textureRef[assetName]--;
            if (_textureRef[assetName] <= 0)
            {
                _textures[assetName].Dispose();
                _textures.Remove(assetName);
                _textureRef.Remove(assetName);
            }
        }

        private static Texture2D? LoadTextureFromFile(string path)
        {
            if (_graphicsDevice == null) throw new InvalidOperationException("AssetManager not initialized with GraphicsDevice.");

            // If path is a content id without extension, try to treat as file with extension fallback
            string filePath = path;
            if (!File.Exists(filePath))
            {
                // try adding common extensions
                var tried = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
                foreach (var ext in tried)
                {
                    var p = path + ext;
                    if (File.Exists(p)) { filePath = p; break; }
                }
            }

            if (!File.Exists(filePath))
            {
                // As a fallback, try to locate the file anywhere under the current working directory
                try
                {
                    var fileName = Path.GetFileName(path);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var found = Directory.GetFiles(Directory.GetCurrentDirectory(), fileName, SearchOption.AllDirectories);
                        if (found != null && found.Length > 0)
                        {
                            filePath = found[0];
                        }
                    }
                }
                catch { }
            }

            if (!File.Exists(filePath)) return null;

            using var fs = File.OpenRead(filePath);
            // Texture2D.FromStream creates texture on the provided GraphicsDevice.
            var tex = Texture2D.FromStream(_graphicsDevice, fs);
            return tex;
        }

        /// <summary>
        /// Acquire a built-in procedural texture that requires no external asset files.
        /// Useful for quick prototyping and games that start without art.
        /// </summary>
        /// <param name="shape">Built-in primitive shape to generate.</param>
        /// <param name="size">Texture width/height in pixels (minimum 1).</param>
        /// <returns>Cached Texture2D instance.</returns>
        public static Texture2D AcquireBuiltinTexture(BuiltinTextureShape shape, int size = 64)
        {
            if (size < 1) throw new ArgumentOutOfRangeException(nameof(size), "Size must be >= 1.");
            if (_graphicsDevice == null) throw new InvalidOperationException("AssetManager not initialized with GraphicsDevice.");

            var key = GetBuiltinTextureKey(shape, size);
            if (_textures.TryGetValue(key, out var existing))
            {
                _textureRef[key]++;
                return existing;
            }

            var texture = CreateBuiltinTexture(shape, size);
            _textures[key] = texture;
            _textureRef[key] = 1;
            return texture;
        }

        /// <summary>
        /// Release a previously acquired built-in texture.
        /// </summary>
        public static void ReleaseBuiltinTexture(BuiltinTextureShape shape, int size = 64)
        {
            if (size < 1) return;
            ReleaseTexture(GetBuiltinTextureKey(shape, size));
        }

        private static string GetBuiltinTextureKey(BuiltinTextureShape shape, int size)
            => $"{BuiltinTexturePrefix}:{shape}:{size}";

        private static Texture2D CreateBuiltinTexture(BuiltinTextureShape shape, int size)
        {
            var texture = new Texture2D(_graphicsDevice!, size, size);
            var pixels = new Color[size * size];

            switch (shape)
            {
                case BuiltinTextureShape.Pixel:
                case BuiltinTextureShape.Square:
                case BuiltinTextureShape.Cube:
                    FillSquare(pixels, size);
                    break;
                case BuiltinTextureShape.Circle:
                    FillCircle(pixels, size);
                    break;
                case BuiltinTextureShape.Triangle:
                    FillTriangle(pixels, size);
                    break;
                case BuiltinTextureShape.Diamond:
                    FillDiamond(pixels, size);
                    break;
                case BuiltinTextureShape.Hexagon:
                    FillHexagon(pixels, size);
                    break;
                case BuiltinTextureShape.Star:
                    FillStar(pixels, size);
                    break;
                case BuiltinTextureShape.Saw:
                    FillSaw(pixels, size);
                    break;
                default:
                    FillSquare(pixels, size);
                    break;
            }

            texture.SetData(pixels);
            return texture;
        }

        private static void FillSquare(Color[] pixels, int size)
        {
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.White;
        }

        private static void FillCircle(Color[] pixels, int size)
        {
            float center = (size - 1) * 0.5f;
            float radius = size * 0.5f;
            float radiusSq = radius * radius;

            for (int y = 0; y < size; y++)
            {
                float dy = y - center;
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    bool inside = (dx * dx) + (dy * dy) <= radiusSq;
                    pixels[(y * size) + x] = inside ? Color.White : Color.Transparent;
                }
            }
        }

        private static void FillTriangle(Color[] pixels, int size)
        {
            if (size == 1)
            {
                pixels[0] = Color.White;
                return;
            }

            float centerX = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                float t = (float)y / (size - 1);
                float halfWidth = t * centerX;
                float minX = centerX - halfWidth;
                float maxX = centerX + halfWidth;

                for (int x = 0; x < size; x++)
                {
                    bool inside = x >= minX && x <= maxX;
                    pixels[(y * size) + x] = inside ? Color.White : Color.Transparent;
                }
            }
        }

        private static void FillDiamond(Color[] pixels, int size)
        {
            float center = (size - 1) * 0.5f;
            float radius = MathF.Max(1f, center);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float md = MathF.Abs(x - center) + MathF.Abs(y - center);
                    pixels[(y * size) + x] = md <= radius ? Color.White : Color.Transparent;
                }
            }
        }

        private static void FillHexagon(Color[] pixels, int size)
        {
            float c = (size - 1) * 0.5f;
            float r = MathF.Max(1f, size * 0.48f);

            var poly = BuildRegularPolygon(6, c, c, r, -MathF.PI / 2f);
            FillPolygon(pixels, size, poly);
        }

        private static void FillStar(Color[] pixels, int size)
        {
            float c = (size - 1) * 0.5f;
            float outer = MathF.Max(1f, size * 0.48f);
            float inner = outer * 0.45f;

            var points = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float a = (-MathF.PI / 2f) + i * (MathF.PI / 5f);
                float r = (i % 2 == 0) ? outer : inner;
                points[i] = new Vector2(c + MathF.Cos(a) * r, c + MathF.Sin(a) * r);
            }

            FillPolygon(pixels, size, points);
        }

        private static void FillSaw(Color[] pixels, int size)
        {
            float c = (size - 1) * 0.5f;
            float baseR = MathF.Max(1f, size * 0.34f);
            float toothR = MathF.Max(baseR + 1f, size * 0.48f);
            const int teeth = 14;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float dist = MathF.Sqrt((dx * dx) + (dy * dy));
                    if (dist > toothR)
                    {
                        pixels[(y * size) + x] = Color.Transparent;
                        continue;
                    }

                    float ang = MathF.Atan2(dy, dx);
                    float wave = MathF.Abs(MathF.Sin(ang * teeth));
                    float threshold = baseR + (toothR - baseR) * wave;
                    pixels[(y * size) + x] = dist <= threshold ? Color.White : Color.Transparent;
                }
            }
        }

        private static Vector2[] BuildRegularPolygon(int sides, float cx, float cy, float radius, float startAngle)
        {
            var points = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = startAngle + (MathF.Tau * i / sides);
                points[i] = new Vector2(cx + MathF.Cos(a) * radius, cy + MathF.Sin(a) * radius);
            }
            return points;
        }

        private static void FillPolygon(Color[] pixels, int size, IReadOnlyList<Vector2> polygon)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = PointInPolygon(new Vector2(x + 0.5f, y + 0.5f), polygon);
                    pixels[(y * size) + x] = inside ? Color.White : Color.Transparent;
                }
            }
        }

        private static bool PointInPolygon(Vector2 point, IReadOnlyList<Vector2> polygon)
        {
            bool inside = false;
            int count = polygon.Count;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];

                bool intersects = ((pi.Y > point.Y) != (pj.Y > point.Y))
                    && (point.X < (pj.X - pi.X) * (point.Y - pi.Y) / ((pj.Y - pi.Y) + float.Epsilon) + pi.X);

                if (intersects) inside = !inside;
            }

            return inside;
        }

        #endregion

        #region Fonts

        public static SpriteFont AcquireFont(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName)) throw new ArgumentNullException(nameof(assetName));
            if (_fonts.TryGetValue(assetName, out var f))
            {
                _fontRef[assetName]++;
                return f;
            }

            if (_content == null)
                throw new InvalidOperationException("AssetManager not initialized with ContentManager.");

            var font = _content.Load<SpriteFont>(assetName);
            _fonts[assetName] = font;
            _fontRef[assetName] = 1;
            return font;
        }

        public static void ReleaseFont(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName)) return;
            if (!_fonts.ContainsKey(assetName)) return;

            _fontRef[assetName]--;
            if (_fontRef[assetName] <= 0)
            {
                // MonoGame does not provide Dispose on SpriteFont — just remove cache
                _fonts.Remove(assetName);
                _fontRef.Remove(assetName);
            }
        }

        #endregion

        #region Audio (SoundEffect)

        public static SoundEffect AcquireSound(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName)) throw new ArgumentNullException(nameof(assetName));
            if (_sfx.TryGetValue(assetName, out var s))
            {
                _sfxRef[assetName]++;
                return s;
            }

            if (_content == null)
                throw new InvalidOperationException("AssetManager not initialized with ContentManager.");

            var sound = _content.Load<SoundEffect>(assetName);
            _sfx[assetName] = sound;
            _sfxRef[assetName] = 1;
            return sound;
        }

        public static void ReleaseSound(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName)) return;
            if (!_sfx.ContainsKey(assetName)) return;

            _sfxRef[assetName]--;
            if (_sfxRef[assetName] <= 0)
            {
                // SoundEffect implements Dispose
                _sfx[assetName].Dispose();
                _sfx.Remove(assetName);
                _sfxRef.Remove(assetName);
            }
        }

        #endregion

        #region Manifest helpers

        /// <summary>
        /// Preloads all assets referenced by the manifest (synchronously).
        /// Useful when changing scenes to avoid hitches later.
        /// </summary>
        public static void PreloadFromManifest(AssetManifest manifest)
        {
            if (manifest == null) return;
            foreach (var t in manifest.Textures) AcquireTexture(t);
            foreach (var f in manifest.Fonts) AcquireFont(f);
            foreach (var a in manifest.Audio) AcquireSound(a);
            // Misc left to caller (tilemaps/prefabs)
        }

        /// <summary>
        /// Asynchronously preload textures (file-based) from manifest using background tasks where possible.
        /// Note: content pipeline loading is still sync and should be done on main thread.
        /// </summary>
        public static async Task PreloadFromManifestAsync(AssetManifest manifest)
        {
            if (manifest == null) return;

            var tasks = new List<Task>();

            // Try to load textures async (file-based)
            foreach (var t in manifest.Textures)
            {
                string path = t;
                if (!File.Exists(path))
                {
                    var tried = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
                    foreach (var ext in tried)
                        if (File.Exists(t + ext)) { path = t + ext; break; }
                }

                if (File.Exists(path))
                {
                    tasks.Add(AcquireTextureAsync(path));
                }
                else
                {
                    // fallback to sync content load (if available)
                    try { AcquireTexture(t); } catch { /* ignore failures for now */ }
                }
            }

            // Fonts & audio should be loaded on main thread (Content.Load). Do it synchronously:
            foreach (var f in manifest.Fonts) AcquireFont(f);
            foreach (var a in manifest.Audio) AcquireSound(a);

            await Task.WhenAll(tasks);
        }

        #endregion

        #region Utilities / Debug

        public static bool IsTextureLoaded(string key) => _textures.ContainsKey(key);
        public static bool IsFontLoaded(string key) => _fonts.ContainsKey(key);
        public static bool IsSoundLoaded(string key) => _sfx.ContainsKey(key);

        /// <summary>
        /// Unload everything and dispose resources.
        /// </summary>
        public static void UnloadAll()
        {
            foreach (var kv in _textures) kv.Value.Dispose();
            _textures.Clear();
            _textureRef.Clear();

            foreach (var kv in _sfx) kv.Value.Dispose();
            _sfx.Clear();
            _sfxRef.Clear();

            _fonts.Clear();
            _fontRef.Clear();

            _asyncTextureLoads.Clear();
        }

        #endregion
    }
}
