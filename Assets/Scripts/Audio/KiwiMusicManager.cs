using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Administrador central de música procedural y bandas sonoras dinámicas para Kiwi Guardian.
/// Reproduce música ambiental específica para cada bioma (Agua, Tierra, Viento, Fuego, Páramo)
/// y cambia dinámicamente a la música épica de combate cuando Kiwi se enfrenta al jefe regional.
/// Todo el audio es sintetizado proceduralmente a 44.1 kHz con volumen nítido y balanceado.
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

    private Coroutine fadeCoroutine;
    private bool isMuted = false;

    private const float TARGET_BGM_VOLUME = 0.82f;
    private const float TARGET_BOSS_VOLUME = 0.88f;

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

        // Asegurar que el AudioListener esté activo y al máximo
        AudioListener.volume = 1f;
        AudioListener.pause = false;

        // Fuente 1: Música ambiental del bioma
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f; // Sonido 2D puro sin atenuación por distancia
        bgmSource.volume = TARGET_BGM_VOLUME;
        bgmSource.playOnAwake = false;

        // Fuente 2: Música de batalla de jefes
        bossSource = gameObject.AddComponent<AudioSource>();
        bossSource.loop = true;
        bossSource.spatialBlend = 0f; // Sonido 2D puro
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
        EnsureAudioListener();
        PlayCurrentSceneMusic(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureAudioListener();
        isBossBattleActive = false;
        if (bossSource != null)
        {
            bossSource.Stop();
            bossSource.volume = 0f;
        }
        if (bgmSource != null)
        {
            bgmSource.volume = isMuted ? 0f : TARGET_BGM_VOLUME;
        }

        PlayCurrentSceneMusic(scene.name);
    }

    private void EnsureAudioListener()
    {
        AudioListener.volume = 1f;
        AudioListener.pause = false;

        AudioListener listener = FindAnyObjectByType<AudioListener>();
        if (listener == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }
            else
            {
                gameObject.AddComponent<AudioListener>();
            }
        }
    }

    public void PlayCurrentSceneMusic(string sceneName)
    {
        currentSceneName = sceneName;
        AudioClip clip = GetOrCreateSceneTrack(sceneName);

        if (clip != null && bgmSource != null)
        {
            if (bgmSource.clip != clip || !bgmSource.isPlaying)
            {
                bgmSource.clip = clip;
                bgmSource.Play();
            }
        }
    }

    private void Update()
    {
        FindPlayer();
        CheckBossProximity();

        // Tecla [N] para silenciar o reactivar la música opcionalmente
        if (Input.GetKeyDown(KeyCode.N))
        {
            isMuted = !isMuted;
            if (bgmSource != null) bgmSource.mute = isMuted;
            if (bossSource != null) bossSource.mute = isMuted;
        }
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

        bool bossNearby = false;

        // 1. Ballena de Plástico (Agua - Mar de Plástico)
        if (PlasticWhaleBossAI.ActiveBoss != null && !PlasticWhaleBossAI.ActiveBoss.IsDead)
        {
            if (Vector2.Distance(playerTransform.position, PlasticWhaleBossAI.ActiveBoss.transform.position) < 22f)
                bossNearby = true;
        }

        // 2. Guardián Pútrido (Tierra - Bosque Marchito)
        if (!bossNearby && GuardianPutridoBossAI.ActiveBoss != null && !GuardianPutridoBossAI.ActiveBoss.IsDead)
        {
            if (Vector2.Distance(playerTransform.position, GuardianPutridoBossAI.ActiveBoss.transform.position) < 22f)
                bossNearby = true;
        }

        // 3. Coloso de Combustión (Fuego - Desierto de Residuos)
        if (!bossNearby && CombustionColossusBossAI.ActiveBoss != null && !CombustionColossusBossAI.ActiveBoss.IsDead)
        {
            if (Vector2.Distance(playerTransform.position, CombustionColossusBossAI.ActiveBoss.transform.position) < 22f)
                bossNearby = true;
        }

        // 4. Superenjambre Tóxico (Viento - Ciudad de Humo)
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

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeTracks(TARGET_BGM_VOLUME * 0.15f, TARGET_BOSS_VOLUME, 1.2f));
    }

    public void StopBossMusic()
    {
        if (!isBossBattleActive) return;
        isBossBattleActive = false;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeTracks(TARGET_BGM_VOLUME, 0f, 1.5f));
    }

    private IEnumerator CrossfadeTracks(float targetBgm, float targetBoss, float duration)
    {
        float startBgm = bgmSource != null ? bgmSource.volume : 0f;
        float startBoss = bossSource != null ? bossSource.volume : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (!isMuted)
            {
                if (bgmSource != null) bgmSource.volume = Mathf.Lerp(startBgm, targetBgm, t);
                if (bossSource != null) bossSource.volume = Mathf.Lerp(startBoss, targetBoss, t);
            }

            yield return null;
        }

        if (targetBoss <= 0.01f && bossSource != null)
        {
            bossSource.Stop();
        }
    }

    private AudioClip GetOrCreateSceneTrack(string sceneName)
    {
        if (sceneMusicCache.TryGetValue(sceneName, out AudioClip cached) && cached != null)
        {
            return cached;
        }

        AudioClip generated = null;

        if (sceneName.Contains("MarDePlastico") || sceneName.Contains("Water") || sceneName.Contains("Agua"))
        {
            // Océano de Plástico (Agua): Melodía fluida, arpegios acuáticos, acordes profundos de misterio marino
            generated = GenerateTheme("BGM_OceanoPlastico",
                new float[] { 174.6f, 196f, 220f, 261.6f, 293.7f, 349.2f, 392f, 440f, 523.2f },
                new int[] { 2, 4, 3, 5,  4, 2, 1, 3,  2, 5, 4, 6,  5, 3, 2, 0 },
                new float[] { 87.3f, 87.3f, 98f, 98f, 110f, 110f, 87.3f, 130.8f },
                0.28f, 0.75f);
        }
        else if (sceneName.Contains("BosqueMarchito") || sceneName.Contains("Forest") || sceneName.Contains("Tierra"))
        {
            // Bosque Marchito (Tierra): Melodía mística y vegetal, ritmo de percusión orgánica, notas en Mi menor
            generated = GenerateTheme("BGM_BosqueMarchito",
                new float[] { 164.8f, 196f, 220f, 246.9f, 293.7f, 329.6f, 392f, 440f },
                new int[] { 0, 2, 4, 5,  4, 3, 2, 1,  0, 3, 5, 6,  5, 4, 2, 0 },
                new float[] { 82.4f, 82.4f, 98f, 98f, 110f, 110f, 82.4f, 123.5f },
                0.26f, 0.78f);
        }
        else if (sceneName.Contains("CiudadDeHumo") || sceneName.Contains("Smog") || sceneName.Contains("Viento"))
        {
            // Ciudad de Humo (Viento/Smog): Melodía industrial, pulsaciones de viento denso, sintetizador futurista
            generated = GenerateTheme("BGM_CiudadDeHumo",
                new float[] { 220f, 246.9f, 261.6f, 293.7f, 329.6f, 349.2f, 440f, 493.9f },
                new int[] { 0, 3, 2, 4,  1, 4, 3, 5,  2, 5, 4, 6,  4, 2, 1, 0 },
                new float[] { 55f, 55f, 73.4f, 73.4f, 65.4f, 65.4f, 82.4f, 73.4f },
                0.22f, 0.80f);
        }
        else if (sceneName.Contains("DesiertoDeResiduos") || sceneName.Contains("Desert") || sceneName.Contains("Fuego"))
        {
            // Desierto de Residuos (Fuego): Escala frigia del desierto, pulsaciones rápidas, calor sofocante en Re menor
            generated = GenerateTheme("BGM_DesiertoResiduos",
                new float[] { 146.8f, 155.6f, 185f, 196f, 220f, 233.1f, 293.7f, 311.1f },
                new int[] { 0, 1, 3, 4,  3, 2, 1, 0,  4, 5, 4, 2,  3, 2, 1, 0 },
                new float[] { 73.4f, 77.8f, 73.4f, 77.8f, 92.5f, 87.3f, 77.8f, 73.4f },
                0.20f, 0.82f);
        }
        else // ParamoToxico o MainMenu
        {
            // Santuario del Árbol de la Vida / Hub Central: Místico, esperanzador, campanillas suaves en Do Mayor / La menor
            generated = GenerateTheme("BGM_SantuarioKiwi",
                new float[] { 261.6f, 293.7f, 329.6f, 392f, 440f, 523.2f, 659.2f },
                new int[] { 0, 2, 3, 5,  4, 2, 3, 4,  2, 0, 1, 2,  0, 2, 4, 5 },
                new float[] { 65.4f, 65.4f, 87.3f, 87.3f, 98f, 98f, 110f, 130.8f },
                0.28f, 0.80f);
        }

        sceneMusicCache[sceneName] = generated;
        return generated;
    }

    private AudioClip GenerateTheme(string trackName, float[] scaleNotes, int[] melodySteps, float[] bassline, float stepDuration, float masterVol)
    {
        int sampleRate = 44100;
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

                // 1. Línea Melódica (onda sinusoidal con sutil vibrato y armónicos retro)
                float vibrato = Mathf.Sin(time * 24f) * 2.5f;
                float leadWave1 = Mathf.Sin(2f * Mathf.PI * (melodyFreq + vibrato) * time);
                float leadWave2 = Mathf.Sin(4f * Mathf.PI * (melodyFreq + vibrato) * time) * 0.25f;
                float leadEnv = Mathf.Sin(t * Mathf.PI); // campana suave por nota

                // 2. Línea de Bajo (onda triangular profunda y consistente)
                float bassPhase = (time * bassFreq) % 1f;
                float bassWave = (Mathf.Abs(bassPhase - 0.5f) * 4f - 1f) * 0.50f;

                // 3. Arpegio retro sutil
                float arpFreq = scaleNotes[(s + (i * 4 / samplesPerStep)) % scaleNotes.Length] * 1.5f;
                float arpWave = Mathf.Sin(2f * Mathf.PI * arpFreq * time) * 0.18f * (1f - t);

                float mix = (leadWave1 + leadWave2) * leadEnv * 0.55f + bassWave * 0.40f + arpWave;
                samples[offset + i] = Mathf.Clamp(mix * masterVol, -0.98f, 0.98f);
            }
        }

        AudioClip clip = AudioClip.Create(trackName, totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateBossBattleTrack()
    {
        int sampleRate = 44100;
        float stepDuration = 0.11f;
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
                float leadWave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * leadFreq * time)) * 0.45f * (1f - t * 0.4f);

                // 2. Bajo galopante rápido
                float bassWave = Mathf.Sin(2f * Mathf.PI * bassFreq * time) * 0.55f;

                // 3. Percusión chiptune en cada paso par
                float beatNoise = (s % 2 == 0) ? (Random.value * 2f - 1f) * Mathf.Max(0f, 1f - t * 8f) * 0.35f : 0f;

                float mix = (leadWave + bassWave + beatNoise) * 0.75f;
                samples[offset + i] = Mathf.Clamp(mix, -0.98f, 0.98f);
            }
        }

        AudioClip clip = AudioClip.Create("BGM_BossBattle_Epic", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnGUI()
    {
        // Indicador discreto en la esquina inferior izquierda
        GUI.color = new Color(1f, 1f, 1f, 0.85f);
        string musicStatus = isMuted ? "🔇 Música: Mute [N]" : (isBossBattleActive ? "⚔️ BGM: Batalla de Jefe" : $"🎵 BGM: {currentSceneName}");
        GUI.Label(new Rect(10, Screen.height - 24, 280, 20), musicStatus);
    }
}
