using System.Text.Json;
using System.Text.Json.Serialization;

namespace RenyulEngine.Assets
{
    /// <summary>
    /// Describes assets that belong to a package/scene.
    /// Paths are content pipeline asset names (without extension) or file-relative paths.
    /// Example:
    ///   Textures: ["textures/crate", "images/bg.png"]
    ///   Fonts: ["fonts/Default"]
    ///   Audio: ["audio/jump"]
    /// </summary>
    public class AssetManifest
    {
        public string Name { get; set; } = "UnnamedManifest";
        public string[] Textures { get; set; } = System.Array.Empty<string>();
        public string[] Fonts { get; set; } = System.Array.Empty<string>();
        public string[] Audio { get; set; } = System.Array.Empty<string>();
        public string[] Misc { get; set; } = System.Array.Empty<string>(); // e.g. JSON prefabs, tilemaps, etc.

        public static AssetManifest FromJson(string json)
            => JsonSerializer.Deserialize<AssetManifest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AssetManifest();

        public string ToJson()
            => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
