using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace RenbokoEngine.Audio
{
    /// <summary>
    /// Generates mono PCM clips at runtime for simple procedural SFX.
    /// </summary>
    public static class ProceduralAudioGenerator
    {
        public static SoundEffect Generate(ProceduralSoundSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var s = settings.ClampSafe();
            int sampleCount = Math.Max(1, (int)(s.DurationSeconds * s.SampleRate));
            byte[] pcm = new byte[sampleCount * sizeof(short)];

            int attackSamples = (int)(s.AttackSeconds * s.SampleRate);
            int decaySamples = (int)(s.DecaySeconds * s.SampleRate);
            int releaseSamples = (int)(s.ReleaseSeconds * s.SampleRate);

            int totalAdsr = attackSamples + decaySamples + releaseSamples;
            if (totalAdsr > sampleCount)
            {
                float scale = sampleCount / (float)totalAdsr;
                attackSamples = (int)(attackSamples * scale);
                decaySamples = (int)(decaySamples * scale);
                releaseSamples = (int)(releaseSamples * scale);
            }

            int sustainSamples = Math.Max(0, sampleCount - (attackSamples + decaySamples + releaseSamples));
            int sustainStart = attackSamples + decaySamples;
            int releaseStart = sustainStart + sustainSamples;

            Random? rng = s.Waveform == ProceduralWaveform.Noise
                ? (s.NoiseSeed.HasValue ? new Random(s.NoiseSeed.Value) : Random.Shared)
                : null;

            double phase = 0d;
            const double twoPi = Math.PI * 2d;

            for (int i = 0; i < sampleCount; i++)
            {
                float progress = sampleCount > 1 ? i / (float)(sampleCount - 1) : 0f;
                float hz = Math.Max(1f, s.FrequencyHz + (s.FrequencySweepHz * progress));
                phase += twoPi * hz / s.SampleRate;

                float wave = s.Waveform switch
                {
                    ProceduralWaveform.Sine => (float)Math.Sin(phase),
                    ProceduralWaveform.Square => Math.Sin(phase) >= 0d ? 1f : -1f,
                    ProceduralWaveform.Triangle => 2f / (float)Math.PI * (float)Math.Asin(Math.Sin(phase)),
                    ProceduralWaveform.Saw => 2f * (float)(phase / twoPi - Math.Floor(phase / twoPi + 0.5d)),
                    ProceduralWaveform.Noise => (float)(rng!.NextDouble() * 2d - 1d),
                    _ => 0f
                };

                float envelope;
                if (attackSamples > 0 && i < attackSamples)
                {
                    envelope = i / (float)attackSamples;
                }
                else if (decaySamples > 0 && i < sustainStart)
                {
                    float decayT = (i - attackSamples) / (float)decaySamples;
                    envelope = 1f + (s.SustainLevel - 1f) * decayT;
                }
                else if (i < releaseStart)
                {
                    envelope = s.SustainLevel;
                }
                else if (releaseSamples > 0)
                {
                    float releaseT = (i - releaseStart) / (float)releaseSamples;
                    envelope = s.SustainLevel * (1f - MathHelper.Clamp(releaseT, 0f, 1f));
                }
                else
                {
                    envelope = 0f;
                }

                float sample = wave * envelope * s.Gain;
                sample = MathHelper.Clamp(sample, -1f, 1f);

                short pcmSample = (short)(sample * short.MaxValue);
                pcm[i * 2] = (byte)(pcmSample & 0xFF);
                pcm[(i * 2) + 1] = (byte)((pcmSample >> 8) & 0xFF);
            }

            return new SoundEffect(pcm, s.SampleRate, AudioChannels.Mono);
        }
    }
}
