using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace RenbokoEngine.Audio
{
    /// <summary>
    /// Represents a group (mixer channel) that controls volume/mute for multiple AudioSources.
    /// </summary>
    public class AudioGroup
    {
        private readonly List<AudioSource> sources = new();

        public string Name { get; private set; }
        public float Volume { get; set; } = 1f;
        public bool Mute { get; set; } = false;

        internal AudioGroup(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Internal registration of sources to this group.
        /// </summary>
        internal void RegisterSource(AudioSource source)
        {
            if (!sources.Contains(source))
                sources.Add(source);
        }

        internal void UnregisterSource(AudioSource source)
        {
            sources.Remove(source);
        }

        internal void ApplyVolumes()
        {
            foreach (var src in sources)
                src.ApplyVolume();
        }
    }
}
