using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Administrador central de música procedural y bandas sonoras dinámicas para Kiwi Guardian.
/// Reproduce música ambiental específica para cada bioma y cambia fluidamente a la música
/// épica de combate cuando Kiwi se enfrenta a los jefes regionales.
/// </summary>
public class KiwiMusicManager : MonoBehaviour
{
    public static KiwiMusicManager Instance { get; private set; }

    private AudioSource bgmSource;
    private AudioSource bossSource;

    private string currentSceneName = "";
    private bool isBossBattleActive = false;
    private Transform playerTransform;

    private static Dictionary<string, AudioClip> sceneMusicCache = new Dictionary<string, AudioClip>();
    private static AudioClip bossBattleClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("KiwiMusicManager");
            Instance = obj.AddComponent<KiwiMusicManager>();
            DontDestroyOnLoad(obj);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = 0.28f;
        bgmSource.playOnAwake = false;

        bossSource = gameObject.AddComponent<AudioSource>();
        bossSource.loop = true;
        bossSource.volume = 0f;
        bossSource.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PlayCurrentSceneMusic(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isBossBattleActive = false;
        bossSource.Stop();
        bossSource.volume = 0f;
        bgmSource.volume = 0.28f;

        PlayCurrentSceneMusic(scene.name);
    }

    public void PlayCurrentSceneMusic(string sceneName)
    {
        currentSceneName = sceneName;
        AudioClip clip = GetOrCreateSceneTrack(sceneName);

        if (clip != null && bgmSource.clip != clip)
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    private void Update()
    {
        FindPlayer();
        CheckBossProximity();
    }

    private void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    private void CheckBossProximity()
    {
        if (playerTransform == null) return;

        // Comprobar si hay un jefe activo cerca de Kiwi
        bool bossNearby = false;

        // 1. Ballena de Plástico
        if (PlasticWhaleBossAI.ActiveBoss != null && !PlasticWhaleBossAI.ActiveBoss.IsDead)
        {
            if (Vector2.Distance(playerTransform.position, PlasticWhaleBossAI.ActiveBoss.transform.position) < 22f)
                bossNearby = true;
        }

        // 2. Guardián Pútrido
        if (!bossNearby && GuardianPutridoBossAI.ActiveBoss != null && !GuardianPutridoBossAI.ActiveBoss.IsDead)
        {
            if (Vector2.Distance(playerTransform.position, GuardianPutridoBossAI.ActiveBoss.transform.position) < 22f)
                bossNearby = true;
        }

        // 3. Coloso de Combustión
        if (!bossNearby && CombustionColossusBossAI.ActiveBoss != null && !CombustionColossusBossAI.ActiveBoss.IsDead)
        {
            if (Vector2.Distance(playerTransform.position, CombustionColossusBossAI.ActiveBoss.transform.position) < 22f)
                bossNearby = true;
        }

        // 4. Superenjambre Tóxico
        if (!bossNearby && ToxicHiveBossAI.ActiveBoss != null && !ToxicHiveBossAI.ActiveBoss.IsDead)
        {
            if (Vector2.Distance(playerTransform.position, ToxicHiveBossAI.ActiveBoss.transform.position) < 24f)
                bossNearby = true;
        }

        if (bossNearby && !isBossBattleActive)
        {
            StartBossMusic();
        }
        else if (!bossNearby && isBossBattleActive)
        {
            StopBossMusic();
        }
    }

    public void StartBossMusic()
    {
        if (isBossBattleActive) return;
        isBossBattleActive = true;

        if (bossBattleClip == null)
        {
            bossBattleClip = GenerateBossBattleTrack();
        }

        bossSource.clip = bossBattleClip;
        bossSource.Play();

        StopAllCoroutines();
        StartCoroutine(Crossfade(bgmSource, bossSource, 0f, 0.38f, 1.2f));
    }

    public void StopBossMusic()
    {
        if (!isBossBattleActive) return;
        isBossBattleActive = false;

        StopAllCoroutines();
        StartCoroutine(Crossfade(bossSource, bgmSource, 0f, 0.28f, 1.5f));
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to, float targetFromVol, float targetToVol, float duration)
    {
        float startFrom = from.volume;
        float startTo = to.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            from.volume = Mathf.Lerp(startFrom, targetFromVol, t);
            to.volume = Mathf.Lerp(startTo, targetToVol, t);
            yield return null;
        }

        from.volume = targetFromVol;
        to.volume = targetToVol;
        if (targetFromVol <= 0.01f) from.Stop();
    }

    // ==========================================
    // GENERADORES PROCEDURALES DE PISTAS MUSICALES RETRO
    // ==========================================

    private AudioClip GetOrCreateSceneTrack(string sceneName)
    {
        if (sceneMusicCache.TryGetValue(sceneName, out AudioClip cached))
            return cached;

        AudioClip generated = null;

        if (sceneName.Contains("MarDePlastico"))
        {
            // Océano: Fluyente, ondas de agua, notas marinas en Fa Mayor / Re menor
            generated = GenerateTheme("BGM_MarDePlastico",
                new float[] { 174.6f, 220f, 261.6f, 349.2f, 392f, 440f, 523.2f },
                new int[] { 0, 2, 4, 2, 5, 4, 3, 2, 1, 3, 4, 3, 2, 1, 0, 2 },
                new float[] { 87.3f, 87.3f, 110f, 110f, 130.8f, 130.8f, 116.5f, 116.5f },
                0.28f, 0.32f);
        }
        else if (sceneName.Contains("BosqueMarchito"))
        {
            // Bosque: Naturaleza ancestral, madera, melodía celta en Mi menor / Sol
            generated = GenerateTheme("BGM_BosqueMarchito",
                new float[] { 164.8f, 196f, 246.9f, 293.7f, 329.6f, 392f, 493.9f },
                new int[] { 0, 1, 3, 4, 3, 1, 4, 5, 4, 3, 1, 2, 0, 1, 3, 0 },
                new float[] { 82.4f, 82.4f, 98f, 98f, 73.4f, 73.4f, 110f, 82.4f },
                0.26f, 0.30f);
        }
        else if (sceneName.Contains("CiudadDeHumo"))
        {
            // Ciudad industrial: Pulso mecánico rítmico en La menor
            generated = GenerateTheme("BGM_CiudadDeHumo",
                new float[] { 220f, 246.9f, 261.6f, 293.7f, 329.6f, 392f, 440f },
                new int[] { 0, 0, 2, 3, 4, 2, 0, 2, 3, 4, 5, 4, 3, 2, 1, 0 },
                new float[] { 55f, 55f, 65.4f, 73.4f, 55f, 55f, 82.4f, 73.4f },
                0.22f, 0.30f);
        }
        else if (sceneName.Contains("DesiertoDeResiduos"))
        {
            // Desierto: Escala frigia exótica, calor tóxico en Re frigio
            generated = GenerateTheme("BGM_DesiertoDeResiduos",
                new float[] { 146.8f, 155.6f, 185f, 196f, 220f, 233f, 293.7f },
                new int[] { 0, 1, 2, 3, 2, 1, 0, 2, 4, 3, 2, 1, 3, 2, 1, 0 },
                new float[] { 73.4f, 77.8f, 73.4f, 77.8f, 92.5f, 87.3f, 77.8f, 73.4f },
                0.24f, 0.30f);
        }
        else // ParamoToxico o MainMenu
        {
            // Santuario: Místico, esperanzador, campanillas suaves en Do Mayor / La menor
            generated = GenerateTheme("BGM_Santuario",
                new float[] { 261.6f, 293.7f, 329.6f, 392f, 440f, 523.2f, 659.2f },
                new int[] { 0, 2, 3, 5, 4, 2, 3, 4, 2, 0, 1, 2, 0, 2, 4, 5 },
                new float[] { 65.4f, 65.4f, 87.3f, 87.3f, 98f, 98f, 110f, 130.8f },
                0.32f, 0.28f);
        }

        sceneMusicCache[sceneName] = generated;
        return generated;
    }

    private AudioClip GenerateTheme(string trackName, float[] scaleNotes, int[] melodySteps, float[] bassline, float stepDuration, float masterVol)
    {
        int sampleRate = 22050;
        int stepsCount = melodySteps.Length;
        int samplesPerStep = Mathf.CeilToInt(sampleRate * stepDuration);
        int totalSamples = samplesPerStep * stepsCount;
        float[] samples = new float[totalSamples];

        for (int s = 0; s < stepsCount; s++)
        {
            float melodyFreq = scaleNotes[melodySteps[s] % scaleNotes.Length];
            float bassFreq = bassline[(s / 2) % bassline.Length];

            int offset = s * samplesPerStep;

            for (int i = 0; i < samplesPerStep; i++)
            {
                float t = (float)i / samplesPerStep;
                float time = (float)(offset + i) / sampleRate;

                // 1. Línea Melódica (onda sinusoidal con sutil vibrato)
                float vibrato = Mathf.Sin(time * 22f) * 3f;
                float leadWave = Mathf.Sin(2f * Mathf.PI * (melodyFreq + vibrato) * time);
                float leadEnv = Mathf.Sin(t * Mathf.PI); // campana suave por nota

                // 2. Línea de Bajo (onda triangular cálida)
                float bassPhase = (time * bassFreq) % 1f;
                float bassWave = (Mathf.Abs(bassPhase - 0.5f) * 4f - 1f) * 0.45f;

                // 3. Toque de arpegio ambiental retro
                float arpFreq = scaleNotes[(s + (i * 4 / samplesPerStep)) % scaleNotes.Length] * 1.5f;
                float arpWave = Mathf.Sin(2f * Mathf.PI * arpFreq * time) * 0.15f * (1f - t);

                float mix = (leadWave * leadEnv * 0.45f) + (bassWave * 0.35f) + arpWave;
                samples[offset + i] = Mathf.Clamp(mix * masterVol, -0.95f, 0.95f);
            }
        }

        AudioClip clip = AudioClip.Create(trackName, totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateBossBattleTrack()
    {
        // Tema de Batalla de Jefe: Ritmo acelerado (150 BPM, 0.10s por paso de 16 notas),
        // bajo continuo galopante en Re menor y notas agudas tensas
        int sampleRate = 22050;
        float stepDuration = 0.105f;
        int stepsCount = 32;
        int samplesPerStep = Mathf.CeilToInt(sampleRate * stepDuration);
        int totalSamples = samplesPerStep * stepsCount;
        float[] samples = new float[totalSamples];

        float[] bossScale = new float[] { 146.8f, 164.8f, 174.6f, 196f, 220f, 233f, 261.6f, 293.7f, 329.6f, 349.2f };
        int[] bossMelody = new int[] {
            7, 7, 6, 7,  8, 7, 5, 4,
            7, 7, 6, 7,  9, 8, 7, 5,
            6, 6, 5, 6,  7, 6, 4, 3,
            5, 4, 3, 2,  4, 3, 2, 0
        };

        float[] bossBass = new float[] { 73.4f, 73.4f, 87.3f, 73.4f, 92.5f, 73.4f, 110f, 98f };

        for (int s = 0; s < stepsCount; s++)
        {
            float leadFreq = bossScale[bossMelody[s] % bossScale.Length];
            float bassFreq = bossBass[(s / 4) % bossBass.Length];
            int offset = s * samplesPerStep;

            for (int i = 0; i < samplesPerStep; i++)
            {
                float t = (float)i / samplesPerStep;
                float time = (float)(offset + i) / sampleRate;

                // 1. Lead punzante estilo onda cuadrada chiptune
                float leadWave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * leadFreq * time)) * 0.35f * (1f - t * 0.5f);

                // 2. Bajo galopante rápido
                float bassWave = Mathf.Sin(2f * Mathf.PI * bassFreq * time) * 0.45f;

                // 3. Percusión chiptune rápida en cada 4 pasos
                float beatNoise = (s % 2 == 0) ? (Random.value * 2f - 1f) * Mathf.Max(0f, 1f - t * 6f) * 0.25f : 0f;

                float mix = (leadWave + bassWave + beatNoise) * 0.32f;
                samples[offset + i] = Mathf.Clamp(mix, -0.95f, 0.95f);
            }
        }

        AudioClip clip = AudioClip.Create("BGM_BossBattle_Dramatic", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
