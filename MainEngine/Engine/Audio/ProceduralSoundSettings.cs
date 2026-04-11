using Microsoft.Xna.Framework;

namespace RenbokoEngine.Audio
{
    /// <summary>
    /// Configuration for generating a procedural one-shot sound effect.
    /// </summary>
    public sealed class ProceduralSoundSettings
    {
        public ProceduralWaveform Waveform { get; init; } = ProceduralWaveform.Sine;

        /// <summary>
        /// Starting frequency in Hz.
        /// </summary>
        public float FrequencyHz { get; init; } = 440f;

        /// <summary>
        /// Frequency delta in Hz applied over the clip duration.
        /// Positive values sweep up, negative values sweep down.
        /// </summary>
        public float FrequencySweepHz { get; init; } = 0f;

        /// <summary>
        /// Total clip duration in seconds.
        /// </summary>
        public float DurationSeconds { get; init; } = 0.2f;

        /// <summary>
        /// Output gain from 0..1.
        /// </summary>
        public float Gain { get; init; } = 0.8f;

        public float AttackSeconds { get; init; } = 0.005f;
        public float DecaySeconds { get; init; } = 0.03f;
        public float SustainLevel { get; init; } = 0.6f;
        public float ReleaseSeconds { get; init; } = 0.04f;

        /// <summary>
        /// Mono PCM sample rate.
        /// </summary>
        public int SampleRate { get; init; } = 44100;

        /// <summary>
        /// Optional noise seed for deterministic generation.
        /// </summary>
        public int? NoiseSeed { get; init; }

        public static ProceduralSoundSettings BasicBeep(float frequencyHz = 880f, float durationSeconds = 0.12f)
        {
            return new ProceduralSoundSettings
            {
                Waveform = ProceduralWaveform.Square,
                FrequencyHz = frequencyHz,
                DurationSeconds = durationSeconds,
                Gain = 0.35f,
                AttackSeconds = 0.001f,
                DecaySeconds = 0.02f,
                SustainLevel = 0.25f,
                ReleaseSeconds = 0.03f
            };
        }

        public static ProceduralSoundSettings BasicBlip(float frequencyHz = 660f)
        {
            return new ProceduralSoundSettings
            {
                Waveform = ProceduralWaveform.Sine,
                FrequencyHz = frequencyHz,
                FrequencySweepHz = 120f,
                DurationSeconds = 0.09f,
                Gain = 0.4f,
                AttackSeconds = 0.001f,
                DecaySeconds = 0.02f,
                SustainLevel = 0.2f,
                ReleaseSeconds = 0.025f
            };
        }

        internal ProceduralSoundSettings ClampSafe()
        {
            return new ProceduralSoundSettings
            {
                Waveform = Waveform,
                FrequencyHz = MathHelper.Clamp(FrequencyHz, 1f, 22050f),
                FrequencySweepHz = FrequencySweepHz,
                DurationSeconds = MathHelper.Clamp(DurationSeconds, 0.005f, 3f),
                Gain = MathHelper.Clamp(Gain, 0f, 1f),
                AttackSeconds = MathHelper.Clamp(AttackSeconds, 0f, 3f),
                DecaySeconds = MathHelper.Clamp(DecaySeconds, 0f, 3f),
                SustainLevel = MathHelper.Clamp(SustainLevel, 0f, 1f),
                ReleaseSeconds = MathHelper.Clamp(ReleaseSeconds, 0f, 3f),
                SampleRate = SampleRate < 8000 ? 8000 : (SampleRate > 48000 ? 48000 : SampleRate),
                NoiseSeed = NoiseSeed
            };
        }
    }
}
