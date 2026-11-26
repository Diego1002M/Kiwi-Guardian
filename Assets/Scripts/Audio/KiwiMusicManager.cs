using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Administrador central de música para Kiwi Guardian.
/// Carga e interpreta bandas sonoras de aventuras retro (estilo plataformero alegre y heroico)
/// descargadas e integradas en Resources/Audio, con fallback procedural alegre en tonalidad Mayor.
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

    private const float TARGET_BGM_VOLUME = 0.75f;
    private const float TARGET_BOSS_VOLUME = 0.82f;

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

        AudioListener.volume = 1f;
        AudioListener.pause = false;

        // Fuente 1: BGM del Bioma
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f; // Sonido 2D
        bgmSource.volume = TARGET_BGM_VOLUME;
        bgmSource.playOnAwake = false;

        // Fuente 2: BGM de Batalla de Jefe
        bossSource = gameObject.AddComponent<AudioSource>();
        bossSource.loop = true;
        bossSource.spatialBlend = 0f;
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

        if (PlasticWhaleBossAI.ActiveBoss != null && !PlasticWhaleBossAI.ActiveBoss.IsDead)
        {
            if (Vector2.Distance(playerTransform.position, PlasticWhaleBossAI.ActiveBoss.transform.position) < 22f)
                bossNearby = true;
        }

        if (!bossNearby && GuardianPutridoBossAI.ActiveBoss != null && !GuardianPutridoBossAI.ActiveBoss.IsDead)
        {
            if (Vector2.Distance(playerTransform.position, GuardianPutridoBossAI.ActiveBoss.transform.position) < 22f)
                bossNearby = true;
        }

        if (!bossNearby && CombustionColossusBossAI.ActiveBoss != null && !CombustionColossusBossAI.ActiveBoss.IsDead)
        {
            if (Vector2.Distance(playerTransform.position, CombustionColossusBossAI.ActiveBoss.transform.position) < 22f)
                bossNearby = true;
        }

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
            bossBattleClip = Resources.Load<AudioClip>("Audio/BGM_Boss_NoEscape");
            if (bossBattleClip == null)
            {
                bossBattleClip = GenerateUpbeatBossTrack();
            }
        }

        bossSource.clip = bossBattleClip;
        bossSource.Play();

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeTracks(TARGET_BGM_VOLUME * 0.15f, TARGET_BOSS_VOLUME, 1.0f));
    }

    public void StopBossMusic()
    {
        if (!isBossBattleActive) return;
        isBossBattleActive = false;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeTracks(TARGET_BGM_VOLUME, 0f, 1.2f));
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

        AudioClip clip = null;

        if (sceneName.Contains("MarDePlastico") || sceneName.Contains("Water") || sceneName.Contains("Agua"))
        {
            // Mar de Plástico (Agua): Plataformero alegre y animado
            clip = Resources.Load<AudioClip>("Audio/BGM_Adventure_PumpUp");
            if (clip == null) clip = GenerateHappyAdventureTrack("BGM_Agua_Alegre", 293.7f); // Re Mayor
        }
        else if (sceneName.Contains("BosqueMarchito") || sceneName.Contains("Forest") || sceneName.Contains("Tierra"))
        {
            // Bosque Marchito (Tierra): Aventura heroica y enérgica
            clip = Resources.Load<AudioClip>("Audio/BGM_Heroic_Intrusion");
            if (clip == null) clip = Resources.Load<AudioClip>("Audio/BGM_Adventure_PumpUp");
            if (clip == null) clip = GenerateHappyAdventureTrack("BGM_Tierra_Heroico", 261.6f); // Do Mayor
        }
        else if (sceneName.Contains("CiudadDeHumo") || sceneName.Contains("Smog") || sceneName.Contains("Viento"))
        {
            // Ciudad de Humo (Viento): Sintetizador aéreo alegre y veloz
            clip = Resources.Load<AudioClip>("Audio/BGM_Air_Rising");
            if (clip == null) clip = GenerateHappyAdventureTrack("BGM_Viento_Aereo", 329.6f); // Mi Mayor
        }
        else if (sceneName.Contains("DesiertoDeResiduos") || sceneName.Contains("Desert") || sceneName.Contains("Fuego") || sceneName.Contains("Lava"))
        {
            // Desierto de Residuos (Fuego / Lava): Ritmo dinámico y activo de acción retro
            clip = Resources.Load<AudioClip>("Audio/BGM_Underground");
            if (clip == null) clip = GenerateHappyAdventureTrack("BGM_Fuego_Accion", 293.7f); // Re Mayor / Mixolidio
        }
        else
        {
            // Páramo Tóxico (Hub Central / Santuario): Melodía alegre, amigable y esperanzadora
            clip = Resources.Load<AudioClip>("Audio/BGM_LoopRide");
            if (clip == null) clip = GenerateHappyAdventureTrack("BGM_Hub_Alegre", 261.6f); // Do Mayor
        }

        if (clip != null)
        {
            sceneMusicCache[sceneName] = clip;
        }

        return clip;
    }

    /// <summary>
    /// Generador procedural de respaldo en Modo Mayor alegre (Estilo Super Mario / Kirby / 8-bit feliz),
    /// garantizando que NUNCA suene tétrico ni como juego de terror.
    /// </summary>
    private AudioClip GenerateHappyAdventureTrack(string trackName, float rootFreq)
    {
        int sampleRate = 44100;
        float stepDuration = 0.14f; // 130 BPM alegre
        int stepsCount = 16;
        int samplesPerStep = Mathf.CeilToInt(sampleRate * stepDuration);
        int totalSamples = samplesPerStep * stepsCount;
        float[] samples = new float[totalSamples];

        // Escala Pentatónica Mayor brillante (Do, Re, Mi, Sol, La, Do agudo)
        float[] scaleNotes = new float[] {
            rootFreq, rootFreq * 1.122f, rootFreq * 1.26f, rootFreq * 1.498f, rootFreq * 1.682f, rootFreq * 2f
        };

        // Melodía saltarina y alegre
        int[] melody = new int[] { 0, 2, 4, 5,  4, 2, 3, 4,  5, 4, 2, 0,  1, 2, 4, 5 };
        // Bajo alegre y saltarín (Tónica y Quinta alternativa)
        float[] bassNotes = new float[] { rootFreq * 0.5f, rootFreq * 0.75f, rootFreq * 0.5f, rootFreq * 0.67f };

        for (int s = 0; s < stepsCount; s++)
        {
            float leadFreq = scaleNotes[melody[s] % scaleNotes.Length];
            float bassFreq = bassNotes[(s / 2) % bassNotes.Length];
            int offset = s * samplesPerStep;

            for (int i = 0; i < samplesPerStep; i++)
            {
                float t = (float)i / samplesPerStep;
                float time = (float)(offset + i) / sampleRate;

                // 1. Lead alegre y brillante tipo onda cuadrada chiptune
                float leadSquare = (Mathf.Sin(2f * Mathf.PI * leadFreq * time) > 0f ? 0.35f : -0.35f) * Mathf.Max(0f, 1f - t * 1.2f);

                // 2. Bajo saltarín acústico
                float bassTri = (Mathf.PingPong(time * bassFreq * 2f, 1f) - 0.5f) * 0.40f * Mathf.Max(0f, 1f - t * 0.9f);

                // 3. Arpegio agudo brillante
                float arpFreq = scaleNotes[(s + (i * 3 / samplesPerStep)) % scaleNotes.Length] * 2f;
                float arpWave = Mathf.Sin(2f * Mathf.PI * arpFreq * time) * 0.15f * (1f - t);

                float mix = (leadSquare + bassTri + arpWave) * 0.75f;
                samples[offset + i] = Mathf.Clamp(mix, -0.95f, 0.95f);
            }
        }

        AudioClip clip = AudioClip.Create(trackName, totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateUpbeatBossTrack()
    {
        int sampleRate = 44100;
        float stepDuration = 0.10f; // 150 BPM trepidante
        int stepsCount = 16;
        int samplesPerStep = Mathf.CeilToInt(sampleRate * stepDuration);
        int totalSamples = samplesPerStep * stepsCount;
        float[] samples = new float[totalSamples];

        float[] bossNotes = new float[] { 220f, 261.6f, 293.7f, 329.6f, 392f, 440f };
        int[] pattern = new int[] { 0, 3, 2, 4, 3, 5, 4, 2, 5, 4, 3, 1, 2, 3, 4, 5 };

        for (int s = 0; s < stepsCount; s++)
        {
            float freq = bossNotes[pattern[s] % bossNotes.Length];
            int offset = s * samplesPerStep;

            for (int i = 0; i < samplesPerStep; i++)
            {
                float t = (float)i / samplesPerStep;
                float time = (float)(offset + i) / sampleRate;

                float lead = (Mathf.Sin(2f * Mathf.PI * freq * time) > 0f ? 0.40f : -0.40f) * (1f - t * 0.8f);
                float snare = (s % 2 == 1) ? (Random.value * 2f - 1f) * Mathf.Max(0f, 1f - t * 6f) * 0.30f : 0f;

                samples[offset + i] = Mathf.Clamp((lead + snare) * 0.8f, -0.95f, 0.95f);
            }
        }

        AudioClip clip = AudioClip.Create("BGM_Boss_Upbeat", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnGUI()
    {
        GUI.color = new Color(1f, 1f, 1f, 0.85f);
        string trackDisplay = bgmSource != null && bgmSource.clip != null ? bgmSource.clip.name : currentSceneName;
        string musicStatus = isMuted ? "🔇 Música: Mute [N]" : (isBossBattleActive ? "⚔️ Jefe: En combate" : $"🎵 {trackDisplay}");
        GUI.Label(new Rect(10, Screen.height - 24, 320, 20), musicStatus);
    }
}
