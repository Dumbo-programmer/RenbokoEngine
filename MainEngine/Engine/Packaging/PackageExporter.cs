using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace RenbokoEngine.Packaging
{
    /// <summary>
    /// Utility to export a package (.renpkg) as a ZIP file.
    /// The package layout:
    ///   manifest.json
    ///   assets/...   (all asset files listed in manifest.Assets)
    ///   bin/...      (optional assemblies listed in manifest.Assemblies)
    /// </summary>
    public static class PackageExporter
    {
        /// <summary>
        /// Export a package to outPackagePath (e.g. "mycontent.renpkg").
        /// - srcRoot is the base folder where asset files and assemblies are located.
        /// - manifest defines which files to include (Assets and Assemblies are relative to srcRoot).
        /// - overwrite: if true, existing package will be replaced.
        /// </summary>
        public static void Export(string srcRoot, string outPackagePath, PackageManifest manifest, bool overwrite = true)
        {
            if (string.IsNullOrWhiteSpace(srcRoot)) throw new ArgumentNullException(nameof(srcRoot));
            if (string.IsNullOrWhiteSpace(outPackagePath)) throw new ArgumentNullException(nameof(outPackagePath));
            if (manifest == null) throw new ArgumentNullException(nameof(manifest));

            if (!Directory.Exists(srcRoot)) throw new DirectoryNotFoundException($"Source root not found: {srcRoot}");

            if (File.Exists(outPackagePath))
            {
                if (overwrite) File.Delete(outPackagePath);
                else throw new IOException($"Output package already exists: {outPackagePath}");
            }

            using var zip = ZipFile.Open(outPackagePath, ZipArchiveMode.Create);

            // Add manifest.json as root entry
            var manifestEntry = zip.CreateEntry("manifest.json");
            using (var ms = new StreamWriter(manifestEntry.Open()))
            {
                ms.Write(manifest.ToJson());
            }

            // Add assets under assets/...
            foreach (var assetRel in manifest.Assets ?? Array.Empty<string>())
            {
                var fullPath = Path.Combine(srcRoot, assetRel);
                if (!File.Exists(fullPath))
                {
                    // try with common extensions if assetRel looks like a content id
                    var found = TryFindWithExtensions(fullPath, out var foundPath);
                    if (!found)
                        throw new FileNotFoundException($"Asset file listed in manifest not found: {assetRel} (searched under {srcRoot})");
                    fullPath = foundPath;
                }

                // preserve the relative path inside the package under assets/
                var entryPath = Path.Combine("assets", assetRel).Replace('\\', '/');
                zip.CreateEntryFromFile(fullPath, entryPath, CompressionLevel.Optimal);
            }

            // Add assemblies into bin/...
            foreach (var asmRel in manifest.Assemblies ?? Array.Empty<string>())
            {
                var asmFull = Path.Combine(srcRoot, asmRel);
                if (!File.Exists(asmFull)) throw new FileNotFoundException($"Assembly listed in manifest not found: {asmRel}");
                var entryPath = Path.Combine("bin", asmRel).Replace('\\', '/');
                zip.CreateEntryFromFile(asmFull, entryPath, CompressionLevel.Optimal);
            }
        }

        /// <summary>
        /// Async wrapper around Export (runs export on a background thread).
        /// Note: this method performs file I/O and zipping and is safe to call from background threads.
        /// </summary>
        public static Task ExportAsync(string srcRoot, string outPackagePath, PackageManifest manifest, bool overwrite = true)
        {
            return Task.Run(() => Export(srcRoot, outPackagePath, manifest, overwrite));
        }

        private static bool TryFindWithExtensions(string pathWithoutExt, out string foundPath)
        {
            var tried = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".ogg", ".wav", ".mp3", ".json", ".txt" };
            foreach (var ext in tried)
            {
                var p = pathWithoutExt + ext;
                if (File.Exists(p))
                {
                    foundPath = p;
                    return true;
                }
            }
            foundPath = string.Empty;
            return false;
        }
    }
}
