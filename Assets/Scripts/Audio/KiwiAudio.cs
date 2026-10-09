using UnityEngine;

/// <summary>
/// Sistema de audio procedural retro/chiptune para Kiwi.
/// Genera efectos de sonido nítidos directamente en memoria sin necesidad de archivos de audio externos.
/// </summary>
public static class KiwiAudio
{
    private static AudioClip jumpClip;
    private static AudioClip doubleJumpClip;
    private static AudioClip daggerClip;
    private static AudioClip hitClip;
    private static AudioClip flyPopClip;
    private static AudioClip seedClip;
    private static AudioClip dashClip;
    private static AudioClip bossRoarClip;
    private static AudioClip victoryClip;

    private static AudioSource audioSource;

    private static void EnsureAudioSource()
    {
        if (audioSource == null)
        {
            GameObject obj = GameObject.Find("KiwiAudioSource");
            if (obj == null)
            {
                obj = new GameObject("KiwiAudioSource");
                Object.DontDestroyOnLoad(obj);
            }
            audioSource = obj.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = obj.AddComponent<AudioSource>();
            }
        }
    }

    public static void PlayJump()
    {
        if (jumpClip == null) jumpClip = CreateChirp(320f, 620f, 0.12f, 0.4f);
        Play(jumpClip);
    }

    public static void PlayDoubleJump()
    {
        if (doubleJumpClip == null) doubleJumpClip = CreateChirp(580f, 1150f, 0.18f, 0.5f);
        Play(doubleJumpClip);
    }

    public static void PlayDagger()
    {
        if (daggerClip == null) daggerClip = CreateNoiseWhoosh(0.1f, 0.35f);
        Play(daggerClip);
    }

    public static void PlayEnemyHit()
    {
        if (hitClip == null) hitClip = CreateChirp(220f, 70f, 0.12f, 0.55f);
        Play(hitClip);
    }

    public static void PlayFlyPop()
    {
        if (flyPopClip == null) flyPopClip = CreateChirp(850f, 250f, 0.11f, 0.5f);
        Play(flyPopClip);
    }

    public static void PlaySeed()
    {
        if (seedClip == null) seedClip = CreateArpeggio(new float[] { 659f, 880f, 1318f }, 0.06f, 0.45f);
        Play(seedClip);
    }

    public static void PlayDash()
    {
        if (dashClip == null) dashClip = CreateNoiseWhoosh(0.22f, 0.5f);
        Play(dashClip);
    }

    public static void PlayBossRoar()
    {
        if (bossRoarClip == null) bossRoarClip = CreateRumble(65f, 0.45f, 0.6f);
        Play(bossRoarClip);
    }

    public static void PlayVictory()
    {
        if (victoryClip == null) victoryClip = CreateArpeggio(new float[] { 523f, 659f, 784f, 1046f }, 0.14f, 0.55f);
        Play(victoryClip);
    }

    private static void Play(AudioClip clip)
    {
        EnsureAudioSource();
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private static AudioClip CreateChirp(float startFreq, float endFreq, float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            float phase = 2f * Mathf.PI * freq * (i / (float)sampleRate);
            float wave = Mathf.Sin(phase);
            float envelope = 1f - t; // Fade out gradual
            samples[i] = wave * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("Chirp", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateNoiseWhoosh(float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Sin(t * Mathf.PI); // campana
            float noise = (Random.value * 2f - 1f);
            samples[i] = noise * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("Whoosh", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateRumble(float baseFreq, float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float phase = 2f * Mathf.PI * baseFreq * (i / (float)sampleRate);
            float wave = Mathf.Sin(phase) + 0.5f * (Random.value * 2f - 1f);
            float envelope = 1f - t;
            samples[i] = Mathf.Clamp(wave, -1f, 1f) * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("Rumble", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateArpeggio(float[] frequencies, float noteDuration, float volume)
    {
        int sampleRate = 44100;
        int samplesPerNote = Mathf.CeilToInt(sampleRate * noteDuration);
        int totalSamples = samplesPerNote * frequencies.Length;
        float[] samples = new float[totalSamples];

        int offset = 0;
        foreach (float freq in frequencies)
        {
            for (int i = 0; i < samplesPerNote; i++)
            {
                float t = (float)i / samplesPerNote;
                float phase = 2f * Mathf.PI * freq * (i / (float)sampleRate);
                float wave = Mathf.Sin(phase);
                float env = 1f - (t * 0.5f);
                samples[offset + i] = wave * env * volume;
            }
            offset += samplesPerNote;
        }

        AudioClip clip = AudioClip.Create("Arpeggio", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
