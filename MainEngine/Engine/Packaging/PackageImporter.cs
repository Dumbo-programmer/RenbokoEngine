using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Loader;

namespace RenyulEngine.Packaging
{
    /// <summary>
    /// Imports a .renpkg (zip) into a target directory.
    /// Returns the deserialized PackageManifest so caller can react (e.g. register assets).
    ///
    /// IMPORTANT SECURITY NOTE:
    /// - The importer will extract files to disk. Do NOT import untrusted packages from unknown sources.
    /// - Loading assemblies from packages is potentially unsafe (arbitrary code execution). We provide an option to
    ///   return assembly paths so the caller can choose to load them explicitly after validation.
    /// </summary>
    public static class PackageImporter
    {
        /// <summary>
        /// Extracts a package and returns the manifest.
        /// - packagePath: path to the .renpkg zip file.
        /// - targetRoot: directory where 'assets/' and 'bin/' (if present) will be extracted.
        /// - overwrite: whether to overwrite existing files in targetRoot.
        /// - returnAssemblyPaths: if true, returns full paths to extracted assemblies (caller may choose to load them).
        /// </summary>
        public static async Task<(PackageManifest Manifest, string[] ExtractedAssetPaths, string[] ExtractedAssemblyPaths)> ImportAsync(
            string packagePath,
            string targetRoot,
            bool overwrite = false,
            bool returnAssemblyPaths = true)
        {
            if (string.IsNullOrWhiteSpace(packagePath)) throw new ArgumentNullException(nameof(packagePath));
            if (string.IsNullOrWhiteSpace(targetRoot)) throw new ArgumentNullException(nameof(targetRoot));
            if (!File.Exists(packagePath)) throw new FileNotFoundException("Package not found", packagePath);

            // Ensure directory exists
            Directory.CreateDirectory(targetRoot);

            var extractedAssets = new List<string>();
            var extractedAssemblies = new List<string>();
            PackageManifest manifest;

            // Use zip archive read in streaming mode
            using (var archive = ZipFile.OpenRead(packagePath))
            {
                // locate manifest.json
                var manifestEntry = archive.GetEntry("manifest.json");
                if (manifestEntry == null)
                    throw new InvalidDataException("Package missing manifest.json");

                using (var ms = new MemoryStream())
                using (var stream = manifestEntry.Open())
                {
                    await stream.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    using var sr = new StreamReader(ms);
                    var manifestJson = await sr.ReadToEndAsync();
                    manifest = PackageManifest.FromJson(manifestJson);
                }

                foreach (var entry in archive.Entries)
                {
                    if (string.Equals(entry.FullName, "manifest.json", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Only allow entries under assets/ or bin/ (safe policy for now)
                    if (!entry.FullName.StartsWith("assets/", StringComparison.OrdinalIgnoreCase) &&
                        !entry.FullName.StartsWith("bin/", StringComparison.OrdinalIgnoreCase))
                    {
                        // Skip unknown top-level files but you could choose to extract them if desired.
                        continue;
                    }

                    var relativePath = entry.FullName.Contains("/") ? entry.FullName.Substring(entry.FullName.IndexOf('/') + 1) : entry.FullName;
                    var destPath = Path.Combine(targetRoot, relativePath);

                    var destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir)) Directory.CreateDirectory(destDir);

                    if (File.Exists(destPath))
                    {
                        if (overwrite) File.Delete(destPath);
                        else continue; // skip extraction to avoid overwrite
                    }

                    // Extract entry to disk
                    entry.ExtractToFile(destPath);

                    if (entry.FullName.StartsWith("assets/", StringComparison.OrdinalIgnoreCase))
                        extractedAssets.Add(destPath);
                    else if (entry.FullName.StartsWith("bin/", StringComparison.OrdinalIgnoreCase))
                        extractedAssemblies.Add(destPath);
                }
            }

            // Return manifest + lists. Caller may choose to load assets or assemblies.
            return (manifest, extractedAssets.ToArray(), returnAssemblyPaths ? extractedAssemblies.ToArray() : Array.Empty<string>());
        }

        /// <summary>
        /// Convenience method to synchronously import (calls ImportAsync and waits).
        /// </summary>
        public static (PackageManifest Manifest, string[] ExtractedAssets, string[] ExtractedAssemblies) Import(string packagePath, string targetRoot, bool overwrite = false, bool returnAssemblyPaths = true)
        {
            return ImportAsync(packagePath, targetRoot, overwrite, returnAssemblyPaths).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Example helper to load extracted assemblies into the default AssemblyLoadContext.
        /// WARNING: loading third-party assemblies will execute code. Validate package/authorship before calling this.
        /// </summary>
        public static Assembly[] LoadAssemblies(string[] assemblyPaths)
        {
            var loaded = new List<Assembly>();
            foreach (var path in assemblyPaths)
            {
                if (!File.Exists(path)) continue;

                // Option 1: AssemblyLoadContext (recommended for .NET Core/.NET 5+)
                try
                {
                    var alc = AssemblyLoadContext.Default;
                    var asm = alc.LoadFromAssemblyPath(Path.GetFullPath(path));
                    loaded.Add(asm);
                }
                catch (Exception ex)
                {
                    // swallow or rethrow based on caller preference – for now we add debug info
                    Console.WriteLine($"Failed to load assembly {path}: {ex.Message}");
                }
            }
            return loaded.ToArray();
        }
    }
}
