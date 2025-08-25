using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RenyulEngine.Packaging
{
    /// <summary>
    /// Manifest stored inside .renpkg packages.
    /// Lists assets (relative paths), assemblies to load, and basic metadata.
    /// </summary>
    public class PackageManifest
    {
        public string Name { get; set; } = "Unnamed Package";
        public string Version { get; set; } = "0.0.0";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";

        /// <summary>
        /// Relative paths in the package under the `assets/` directory.
        /// Example: ["textures/crate.png", "prefabs/enemy.json"]
        /// </summary>
        public string[] Assets { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Relative paths under `bin/` that contain compiled assemblies (dll) that the package ships.
        /// These are optional and will not be loaded automatically by the importer unless you choose to.
        /// </summary>
        public string[] Assemblies { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Optional: other metadata for package managers.
        /// </summary>
        public string[] Tags { get; set; } = Array.Empty<string>();

        public static PackageManifest FromJson(string json)
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<PackageManifest>(json, opts) ?? new PackageManifest();
        }

        public string ToJson()
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, opts);
        }
    }
}
