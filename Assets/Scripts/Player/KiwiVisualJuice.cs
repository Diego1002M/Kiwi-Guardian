using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de efectos visuales y partículas procedurales ("Game Feel / Juice") para Kiwi:
/// - Polvo pixel art al saltar y aterrizar.
/// - Chispas elementales radiantes al golpear enemigos con dagas.
/// - Destellos dorados al recolectar Semillas de Vida.
/// Funciona de forma ultraligera creando micro-partículas que rotan, desaceleran y se desvanecen.
/// </summary>
public class KiwiVisualJuice : MonoBehaviour
{
    public static KiwiVisualJuice Instance { get; private set; }

    private static Sprite pixelSprite;

    private class ParticleItem
    {
        public GameObject go;
        public SpriteRenderer sr;
        public Vector2 velocity;
        public float life;
        public float maxLife;
        public float startScale;
        public float rotSpeed;
    }

    private List<ParticleItem> activeParticles = new List<ParticleItem>();
    private Transform particleContainer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("KiwiVisualJuice");
            Instance = obj.AddComponent<KiwiVisualJuice>();
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

        particleContainer = new GameObject("Juice_ParticleContainer").transform;
        particleContainer.SetParent(transform);

        if (pixelSprite == null)
        {
            Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color[] colors = new Color[16];
            for (int i = 0; i < 16; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            pixelSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
        }
    }

    /// <summary>
    /// Genera una pequeña nube de polvo al despegar o saltar.
    /// </summary>
    public static void SpawnJumpDust(Vector3 position, Color dustColor)
    {
        if (Instance == null) return;
        for (int i = 0; i < 4; i++)
        {
            Vector2 vel = new Vector2(Random.Range(-1.6f, 1.6f), Random.Range(0.2f, 1.2f));
            Instance.CreateParticle(position + new Vector3(Random.Range(-0.2f, 0.2f), -0.1f, 0), vel, dustColor, Random.Range(0.2f, 0.35f), Random.Range(0.6f, 0.9f));
        }
    }

    /// <summary>
    /// Genera polvo expansivo a ambos lados cuando Kiwi aterriza de una caída.
    /// </summary>
    public static void SpawnLandDust(Vector3 position)
    {
        if (Instance == null) return;
        Color dustColor = new Color(0.85f, 0.85f, 0.8f, 0.85f);
        for (int i = 0; i < 6; i++)
        {
            float side = (i % 2 == 0) ? 1f : -1f;
            Vector2 vel = new Vector2(side * Random.Range(1.2f, 2.8f), Random.Range(0.4f, 1.4f));
            Instance.CreateParticle(position + new Vector3(side * 0.15f, -0.2f, 0), vel, dustColor, Random.Range(0.25f, 0.4f), Random.Range(0.7f, 1.1f));
        }
    }

    /// <summary>
    /// Genera chispas radiantes al golpear un enemigo con la daga.
    /// </summary>
    public static void SpawnHitSparks(Vector3 position, Color elementColor)
    {
        if (Instance == null) return;
        for (int i = 0; i < 8; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = Random.Range(2.5f, 5.5f);
            Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            Instance.CreateParticle(position, vel, elementColor, Random.Range(0.25f, 0.45f), Random.Range(0.8f, 1.2f));
        }
    }

    /// <summary>
    /// Genera un estallido de destellos dorados al recolectar una semilla de vida.
    /// </summary>
    public static void SpawnSeedCollectEffect(Vector3 position)
    {
        if (Instance == null) return;
        Color gold = new Color(1f, 0.9f, 0.2f, 0.95f);
        Color emerald = new Color(0.2f, 1f, 0.6f, 0.95f);

        for (int i = 0; i < 10; i++)
        {
            Vector2 vel = new Vector2(Random.Range(-2.2f, 2.2f), Random.Range(1.5f, 4.2f));
            Color c = (i % 2 == 0) ? gold : emerald;
            Instance.CreateParticle(position, vel, c, Random.Range(0.4f, 0.65f), Random.Range(0.9f, 1.4f));
        }
    }

    private void CreateParticle(Vector3 pos, Vector2 vel, Color col, float life, float scale)
    {
        GameObject pGo = new GameObject("JuicePixel");
        pGo.transform.SetParent(particleContainer);
        pGo.transform.position = pos;
        pGo.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = pGo.AddComponent<SpriteRenderer>();
        sr.sprite = pixelSprite;
        sr.color = col;
        sr.sortingOrder = 200; // Por encima de Kiwi y enemigos

        ParticleItem item = new ParticleItem
        {
            go = pGo,
            sr = sr,
            velocity = vel,
            life = life,
            maxLife = life,
            startScale = scale,
            rotSpeed = Random.Range(-280f, 280f)
        };

        activeParticles.Add(item);
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            ParticleItem p = activeParticles[i];
            p.life -= dt;

            if (p.life <= 0f || p.go == null)
            {
                if (p.go != null) Destroy(p.go);
                activeParticles.RemoveAt(i);
                continue;
            }

            // Física de partículas: movimiento, fricción y gravedad
            p.go.transform.position += (Vector3)p.velocity * dt;
            p.velocity.y -= 7.5f * dt; // Gravedad
            p.velocity.x = Mathf.Lerp(p.velocity.x, 0f, dt * 4f);

            // Rotación
            p.go.transform.Rotate(0, 0, p.rotSpeed * dt);

            // Escala y desvanecimiento progresivo
            float t = p.life / p.maxLife;
            p.go.transform.localScale = Vector3.one * (p.startScale * t);

            if (p.sr != null)
            {
                Color c = p.sr.color;
                c.a = t;
                p.sr.color = c;
            }
        }
    }
}
