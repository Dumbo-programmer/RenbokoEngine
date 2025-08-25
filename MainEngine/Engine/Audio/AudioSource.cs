using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
namespace RenyulEngine.Audio
{
    public class AudioSource
    {
        private readonly SoundEffect sound;
        private SoundEffectInstance instance;

        public bool Loop { get; set; } = false;
        public bool Mute { get; set; } = false;
        public float Volume { get; set; } = 1f;
        public int Priority { get; set; } = 0;

        public AudioGroup Group { get; private set; }

        public bool IsPlaying => instance?.State == SoundState.Playing;

        public AudioSource(SoundEffect sound, string groupName = "Default")
        {
            this.sound = sound;
            instance = sound.CreateInstance();
            instance.IsLooped = Loop;

            AudioSystem.RegisterSource(this);

            Group = AudioSystem.GetOrCreateGroup(groupName);
            Group.RegisterSource(this);
        }

        public void Play()
        {
            if (instance == null)
                return;

            instance.IsLooped = Loop;
            ApplyVolume();
            instance.Play();
        }

        public void Stop() => instance?.Stop();
        public void Pause() => instance?.Pause();
        public void Resume()
        {
            if (instance != null && instance.State == SoundState.Paused)
                instance.Resume();
        }

        public void ApplyVolume()
        {
            if (instance == null) return;

            float groupVolume = Group?.Volume ?? 1f;
            bool groupMute = Group?.Mute ?? false;

            float finalVolume = Volume
                                * (Mute ? 0f : 1f)
                                * (groupMute ? 0f : 1f)
                                * groupVolume
                                * (AudioSystem.Muted ? 0f : 1f)
                                * AudioSystem.MasterVolume;

            instance.Volume = MathHelper.Clamp(finalVolume, 0f, 1f);
        }

        public void Dispose()
        {
            Stop();
            instance?.Dispose();
            AudioSystem.UnregisterSource(this);
            Group?.UnregisterSource(this);
        }
    }
}
