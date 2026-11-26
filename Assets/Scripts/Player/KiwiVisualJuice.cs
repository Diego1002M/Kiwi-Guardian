using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema avanzado de efectos visuales y "Game Feel" para Kiwi (estilo Celeste / Hollow Knight):
/// - Polvo dinámico suave y expansivo bajo los pies al correr, saltar y aterrizar.
/// - Estela de sombras fantasma (Afterimage Ghost Trail) durante el Torbellino / Dash.
/// - Arco de tajo radiante de corte al lanzar dagas.
/// - Chispas elementales de impacto y destellos dorados en semillas de vida.
/// </summary>
public class KiwiVisualJuice : MonoBehaviour
{
    public static KiwiVisualJuice Instance { get; private set; }

    private static Sprite pixelSprite;
    private static Sprite dustPuffSprite;

    private class ParticleItem
    {
        public GameObject go;
        public SpriteRenderer sr;
        public Vector2 velocity;
        public float life;
        public float maxLife;
        public float startScale;
        public float startAlpha;
        public float rotSpeed;
        public bool isGhost;
        public bool isDust;
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

        EnsureSprites();
    }

    private static void EnsureSprites()
    {
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

        if (dustPuffSprite == null)
        {
            int size = 24;
            Texture2D puffTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] cols = new Color[size * size];
            float center = (size - 1) / 2f;
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float normDist = Mathf.Clamp01(dist / radius);
                    // Curva de densidad suave y esponjosa estilo bocanada de nube
                    float alpha = Mathf.Clamp01(1f - normDist);
                    alpha = alpha * alpha * (3f - 2f * alpha); // Smoothstep
                    cols[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            puffTex.SetPixels(cols);
            puffTex.filterMode = FilterMode.Bilinear;
            puffTex.Apply();

            dustPuffSprite = Sprite.Create(puffTex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }
    }

    /// <summary>
    /// Genera bocanadas de polvo visibles y esponjosas bajo los pies de Kiwi al correr por el suelo (estilo Celeste).
    /// </summary>
    public static void SpawnRunDust(Vector3 footPos, bool isFacingRight)
    {
        if (Instance == null) return;
        EnsureSprites();

        float side = isFacingRight ? -1f : 1f;
        Color dustCol = new Color(0.96f, 0.94f, 0.90f, 0.88f); // Polvo claro y muy visible

        // Generar 2 a 3 nubes de polvo por pisada con ligero abanico hacia atrás
        int count = Random.Range(2, 4);
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = footPos + new Vector3(side * (0.08f + i * 0.08f) + Random.Range(-0.04f, 0.04f), Random.Range(0.01f, 0.06f), 0f);
            Vector2 vel = new Vector2(side * Random.Range(0.8f, 2.2f), Random.Range(0.35f, 1.1f));
            float life = Random.Range(0.36f, 0.52f);
            float scale = Random.Range(0.28f, 0.44f);
            Instance.CreateDustParticle(spawnPos, vel, dustCol, life, scale);
        }
    }

    /// <summary>
    /// Genera una explosión expansiva de polvo al saltar o usar doble salto.
    /// </summary>
    public static void SpawnJumpDust(Vector3 position, Color dustColor)
    {
        if (Instance == null) return;
        EnsureSprites();

        dustColor.a = 0.92f;
        for (int i = 0; i < 5; i++)
        {
            float side = (i % 2 == 0) ? 1f : -1f;
            Vector3 spawnPos = position + new Vector3(side * Random.Range(0.05f, 0.22f), Random.Range(0.01f, 0.06f), 0f);
            Vector2 vel = new Vector2(side * Random.Range(1.2f, 2.6f), Random.Range(0.4f, 1.4f));
            float life = Random.Range(0.40f, 0.60f);
            float scale = Random.Range(0.32f, 0.50f);
            Instance.CreateDustParticle(spawnPos, vel, dustColor, life, scale);
        }
    }

    /// <summary>
    /// Genera polvo expansivo a ambos lados cuando Kiwi aterriza en el suelo tras una caída.
    /// </summary>
    public static void SpawnLandDust(Vector3 position)
    {
        if (Instance == null) return;
        EnsureSprites();

        Color dustColor = new Color(0.95f, 0.94f, 0.90f, 0.92f);
        for (int i = 0; i < 8; i++)
        {
            float side = (i % 2 == 0) ? 1f : -1f;
            Vector3 spawnPos = position + new Vector3(side * Random.Range(0.08f, 0.32f), Random.Range(0.01f, 0.06f), 0f);
            Vector2 vel = new Vector2(side * Random.Range(1.8f, 3.8f), Random.Range(0.5f, 1.6f));
            float life = Random.Range(0.44f, 0.68f);
            float scale = Random.Range(0.36f, 0.56f);
            Instance.CreateDustParticle(spawnPos, vel, dustColor, life, scale);
        }
    }

    /// <summary>
    /// Genera una silueta fantasma translúcida (Afterimage / Ghost Trail) durante el Torbellino/Dash.
    /// </summary>
    public static void SpawnDashGhost(Vector3 pos, Sprite sprite, bool flipX, Vector3 scale, Color ghostColor)
    {
        if (Instance == null || sprite == null) return;

        GameObject ghost = new GameObject("DashGhost");
        ghost.transform.SetParent(Instance.particleContainer);
        ghost.transform.position = pos;
        ghost.transform.localScale = scale;

        SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.flipX = flipX;
        sr.color = ghostColor;
        sr.sortingOrder = 4; // Por detrás de Kiwi pero sobre el fondo

        ParticleItem item = new ParticleItem
        {
            go = ghost,
            sr = sr,
            velocity = Vector2.zero,
            life = 0.24f,
            maxLife = 0.24f,
            startScale = scale.x,
            startAlpha = ghostColor.a,
            rotSpeed = 0f,
            isGhost = true,
            isDust = false
        };

        Instance.activeParticles.Add(item);
    }

    /// <summary>
    /// Genera un arco de partículas en semicírculo al realizar un ataque con la daga.
    /// </summary>
    public static void SpawnSlashArc(Vector3 attackPos, bool isFacingRight, Color slashColor)
    {
        if (Instance == null) return;
        EnsureSprites();

        float dir = isFacingRight ? 1f : -1f;
        for (int i = 0; i < 6; i++)
        {
            float angle = (-35f + i * 14f) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle) * 0.55f * dir, Mathf.Sin(angle) * 0.45f);
            Vector2 vel = new Vector2(dir * Random.Range(2.0f, 4.2f), Random.Range(-0.8f, 0.8f));
            Instance.CreateParticle(attackPos + (Vector3)offset, vel, slashColor, 0.20f, Random.Range(0.7f, 1.1f));
        }
    }

    /// <summary>
    /// Genera chispas radiantes al golpear un enemigo con la daga.
    /// </summary>
    public static void SpawnHitSparks(Vector3 position, Color elementColor)
    {
        if (Instance == null) return;
        EnsureSprites();

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
        EnsureSprites();

        Color gold = new Color(1f, 0.9f, 0.2f, 0.95f);
        Color emerald = new Color(0.2f, 1f, 0.6f, 0.95f);

        for (int i = 0; i < 10; i++)
        {
            Vector2 vel = new Vector2(Random.Range(-2.2f, 2.2f), Random.Range(1.5f, 4.2f));
            Color c = (i % 2 == 0) ? gold : emerald;
            Instance.CreateParticle(position, vel, c, Random.Range(0.4f, 0.65f), Random.Range(0.9f, 1.4f));
        }
    }

    private void CreateDustParticle(Vector3 pos, Vector2 vel, Color col, float life, float scale)
    {
        GameObject pGo = new GameObject("JuiceDustPuff");
        pGo.transform.SetParent(particleContainer);
        pGo.transform.position = pos;
        pGo.transform.localScale = Vector3.one * (scale * 0.35f);

        SpriteRenderer sr = pGo.AddComponent<SpriteRenderer>();
        sr.sprite = dustPuffSprite;
        sr.color = col;
        sr.sortingOrder = 15; // Por encima de plataformas para que destaque claramente

        ParticleItem item = new ParticleItem
        {
            go = pGo,
            sr = sr,
            velocity = vel,
            life = life,
            maxLife = life,
            startScale = scale,
            startAlpha = col.a,
            rotSpeed = Random.Range(-40f, 40f),
            isGhost = false,
            isDust = true
        };

        activeParticles.Add(item);
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
        sr.sortingOrder = 200;

        ParticleItem item = new ParticleItem
        {
            go = pGo,
            sr = sr,
            velocity = vel,
            life = life,
            maxLife = life,
            startScale = scale,
            startAlpha = col.a,
            rotSpeed = Random.Range(-280f, 280f),
            isGhost = false,
            isDust = false
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

            float t = p.life / p.maxLife; // 1 -> 0
            float age = 1f - t;           // 0 -> 1

            if (p.isGhost)
            {
                // Para las sombras fantasma: desvanecer alfa suavemente
                if (p.sr != null)
                {
                    Color c = p.sr.color;
                    c.a = t * p.startAlpha;
                    p.sr.color = c;
                }
            }
            else if (p.isDust)
            {
                // Animación de nube de polvo estilo Celeste:
                // 1. En el primer 22% de su vida, se infla rápidamente (pop)
                // 2. Luego flota ligeramente hacia arriba y se expande mientras se desvanece
                p.go.transform.position += (Vector3)p.velocity * dt;
                p.velocity.y += 0.35f * dt; // Flota suavemente hacia arriba
                p.velocity.x = Mathf.Lerp(p.velocity.x, 0f, dt * 3.5f); // Fricción horizontal
                p.go.transform.Rotate(0, 0, p.rotSpeed * dt);

                float currentScale = (age < 0.22f)
                    ? Mathf.Lerp(p.startScale * 0.35f, p.startScale, age / 0.22f)
                    : Mathf.Lerp(p.startScale, p.startScale * 1.35f, (age - 0.22f) / 0.78f);

                p.go.transform.localScale = Vector3.one * currentScale;

                if (p.sr != null)
                {
                    Color c = p.sr.color;
                    c.a = p.startAlpha * Mathf.Clamp01(t * 1.2f);
                    p.sr.color = c;
                }
            }
            else
            {
                // Para las partículas físicas: movimiento, fricción y gravedad
                p.go.transform.position += (Vector3)p.velocity * dt;
                p.velocity.y -= 7.5f * dt;
                p.velocity.x = Mathf.Lerp(p.velocity.x, 0f, dt * 4f);

                p.go.transform.Rotate(0, 0, p.rotSpeed * dt);
                p.go.transform.localScale = Vector3.one * (p.startScale * t);

                if (p.sr != null)
                {
                    Color c = p.sr.color;
                    c.a = t * p.startAlpha;
                    p.sr.color = c;
                }
            }
        }
    }
}
