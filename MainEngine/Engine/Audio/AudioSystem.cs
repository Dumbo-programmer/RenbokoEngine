using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace RenbokoEngine.Audio
{
    /// <summary>
    /// Central audio manager. Handles global volume, muting, groups, and registered sources.
    /// </summary>
    public static class AudioSystem
    {
        private static readonly List<AudioSource> activeSources = new();
        private static readonly Dictionary<string, AudioGroup> groups = new();

        private static float masterVolume = 1f;
        private static bool isMuted = false;

        public static float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = MathHelper.Clamp(value, 0f, 1f);
                UpdateVolumes();
            }
        }

        public static bool Muted
        {
            get => isMuted;
            set
            {
                isMuted = value;
                UpdateVolumes();
            }
        }

        internal static void RegisterSource(AudioSource source)
        {
            if (!activeSources.Contains(source))
                activeSources.Add(source);
        }

        internal static void UnregisterSource(AudioSource source)
        {
            activeSources.Remove(source);
        }

        /// <summary>
        /// Create or get an audio group by name.
        /// </summary>
        public static AudioGroup GetOrCreateGroup(string name)
        {
            if (!groups.ContainsKey(name))
                groups[name] = new AudioGroup(name);
            return groups[name];
        }

        /// <summary>
        /// Get an existing audio group, null if not found.
        /// </summary>
        public static AudioGroup? GetGroup(string name)
        {
            return groups.TryGetValue(name, out var g) ? g : null;
        }

        public static void UpdateVolumes()
        {
            foreach (var src in activeSources)
                src.ApplyVolume();
        }

        /// <summary>
        /// Create a SoundEffect at runtime using procedural synthesis settings.
        /// The caller is responsible for disposing the returned SoundEffect.
        /// </summary>
        public static SoundEffect CreateProceduralSound(ProceduralSoundSettings settings)
        {
            return ProceduralAudioGenerator.Generate(settings);
        }

        /// <summary>
        /// Create an AudioSource backed by a generated procedural SoundEffect.
        /// The created source owns and disposes the generated SoundEffect.
        /// </summary>
        public static AudioSource CreateProceduralSource(ProceduralSoundSettings settings, string groupName = "Default")
        {
            var sound = ProceduralAudioGenerator.Generate(settings);
            return new AudioSource(sound, groupName, ownsSound: true);
        }

        /// <summary>
        /// Convenience helper for short one-shot procedural sounds.
        /// </summary>
        public static void PlayProceduralOneShot(ProceduralSoundSettings settings)
        {
            using var sound = ProceduralAudioGenerator.Generate(settings);
            float volume = Muted ? 0f : MasterVolume;
            sound.Play(volume, 0f, 0f);
        }
    }
}
