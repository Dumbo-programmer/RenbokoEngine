namespace RenbokoEngine.Audio
{
    /// <summary>
    /// Small library of ready-to-use procedural SFX settings.
    /// </summary>
    public static class ProceduralSfxPresets
    {
        public static ProceduralSoundSettings Jump()
        {
            return new ProceduralSoundSettings
            {
                Waveform = ProceduralWaveform.Square,
                FrequencyHz = 520f,
                FrequencySweepHz = 220f,
                DurationSeconds = 0.11f,
                Gain = 0.33f,
                AttackSeconds = 0.001f,
                DecaySeconds = 0.02f,
                SustainLevel = 0.22f,
                ReleaseSeconds = 0.03f
            };
        }

        public static ProceduralSoundSettings Pickup()
        {
            return new ProceduralSoundSettings
            {
                Waveform = ProceduralWaveform.Triangle,
                FrequencyHz = 840f,
                FrequencySweepHz = 360f,
                DurationSeconds = 0.10f,
                Gain = 0.30f,
                AttackSeconds = 0.001f,
                DecaySeconds = 0.02f,
                SustainLevel = 0.18f,
                ReleaseSeconds = 0.025f
            };
        }

        public static ProceduralSoundSettings Crash()
        {
            return new ProceduralSoundSettings
            {
                Waveform = ProceduralWaveform.Noise,
                FrequencyHz = 180f,
                FrequencySweepHz = -120f,
                DurationSeconds = 0.22f,
                Gain = 0.32f,
                AttackSeconds = 0.001f,
                DecaySeconds = 0.04f,
                SustainLevel = 0.15f,
                ReleaseSeconds = 0.09f,
                NoiseSeed = 1337
            };
        }

        public static ProceduralSoundSettings UiConfirm()
        {
            return new ProceduralSoundSettings
            {
                Waveform = ProceduralWaveform.Sine,
                FrequencyHz = 720f,
                FrequencySweepHz = 120f,
                DurationSeconds = 0.075f,
                Gain = 0.26f,
                AttackSeconds = 0.001f,
                DecaySeconds = 0.015f,
                SustainLevel = 0.2f,
                ReleaseSeconds = 0.02f
            };
        }

        public static ProceduralSoundSettings PauseToggle()
        {
            return new ProceduralSoundSettings
            {
                Waveform = ProceduralWaveform.Saw,
                FrequencyHz = 380f,
                FrequencySweepHz = -80f,
                DurationSeconds = 0.08f,
                Gain = 0.20f,
                AttackSeconds = 0.001f,
                DecaySeconds = 0.02f,
                SustainLevel = 0.18f,
                ReleaseSeconds = 0.02f
            };
        }
    }
}
