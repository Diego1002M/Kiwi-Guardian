using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Jefe Regional del Bosque Marchito: El Guardián Pútrido (Espíritu de la Tierra/Bosque Corrompido).
/// Combate de 2 fases con erupción de raíces punzantes, ráfagas de astillas y lluvia de esporas.
/// DEBILIDAD CRÍTICA AL FUEGO: Las dagas de fuego [2 / Q] le causan daño doble (2x).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GuardianPutridoBossAI : MonoBehaviour, IDamageable, IElementalTarget
{
    [Header("Estadísticas del Jefe")]
    [SerializeField] private int maxHealth = 25;
    [SerializeField] private float walkSpeed = 2.4f;
    [SerializeField] private float rageWalkSpeed = 4.2f;
    [SerializeField] private float arenaMinX = 18f;
    [SerializeField] private float arenaMaxX = 31f;

    [Header("Ataques")]
    [SerializeField] private float attackInterval = 3.0f;
    [SerializeField] private float rageAttackInterval = 1.8f;
    [SerializeField] private Sprite projectileSprite;
    [SerializeField] private Sprite rewardSeedSprite;

    private int currentHealth;
    private bool isRaging = false;
    private bool isDead = false;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;
    private float nextSlamTime = 0f;
    private int moveDirection = 1;

    private Transform playerTransform;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private Color originalColor;

    // Sprites dedicados de ataques (para evitar que el jefe se clone a sí mismo)
    private Sprite spikeSprite;
    private Sprite thornSprite;
    private Sprite shockwaveSprite;

    public static GuardianPutridoBossAI ActiveBoss { get; private set; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public ElementType TargetElement => ElementType.Tierra;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.gravityScale = 0f;

        currentHealth = maxHealth;
        ActiveBoss = this;

        // Cargar sprites dedicados de ataques
        spikeSprite = Resources.Load<Sprite>("Sprites/RootSpikeHazard");
        thornSprite = Resources.Load<Sprite>("Sprites/ThornProjectile");
        shockwaveSprite = Resources.Load<Sprite>("Sprites/EarthquakeShockwave");
        if (rewardSeedSprite == null) rewardSeedSprite = Resources.Load<Sprite>("Sprites/EnergySeed");
    }

    private void Start()
    {
        FindPlayer();
        nextAttackTime = Time.time + 2.0f;
        nextSlamTime = Time.time + 5.0f;
    }

    private void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    private void Update()
    {
        if (isDead) return;

        FindPlayer();
        if (playerTransform == null) return;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distToPlayer > 18f) return;

        // 1. Patrulla del Guardián por la arena
        if (!isAttacking)
        {
            float speed = isRaging ? rageWalkSpeed : walkSpeed;
            transform.Translate(Vector3.right * moveDirection * speed * Time.deltaTime, Space.World);

            if (transform.position.x <= arenaMinX)
            {
                moveDirection = 1;
                if (sr != null) sr.flipX = false;
            }
            else if (transform.position.x >= arenaMaxX)
            {
                moveDirection = -1;
                if (sr != null) sr.flipX = true;
            }
        }

        // 2. Patrones de Ataque del Jefe
        float currentInterval = isRaging ? rageAttackInterval : attackInterval;
        if (Time.time >= nextAttackTime && !isAttacking)
        {
            int atk = Random.Range(0, 3);
            if (atk == 0) StartCoroutine(RootEruptionAttack());
            else if (atk == 1) ShootThornBarrage();
            else ShootSporeRain();

            nextAttackTime = Time.time + currentInterval;
        }

        // 3. Fase de Furia: Terremoto sísmico con raíces
        if (isRaging && Time.time >= nextSlamTime && !isAttacking)
        {
            StartCoroutine(EarthquakeSlamAttack());
            nextSlamTime = Time.time + 5.5f;
        }
    }

    // Ataque 1: Erupción de Raíces Punzantes (utiliza sprite dedicado de raíz, nunca el del jefe)
    private IEnumerator RootEruptionAttack()
    {
        isAttacking = true;
        KiwiAudio.PlayEnemyHit();

        Vector3 targetPos = playerTransform.position;

        // Aviso visual en el suelo (círculo rojo / advertencia)
        GameObject warningTelegraph = new GameObject("AvisoRaiz");
        warningTelegraph.transform.position = new Vector3(targetPos.x, targetPos.y - 0.45f, 0f);
        warningTelegraph.transform.localScale = new Vector3(1.2f, 0.25f, 1f);
        SpriteRenderer wSr = warningTelegraph.AddComponent<SpriteRenderer>();
        wSr.sprite = thornSprite != null ? thornSprite : Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), Vector2.one * 0.5f);
        wSr.color = new Color(1f, 0.2f, 0.1f, 0.5f);

        yield return new WaitForSeconds(0.6f);
        Destroy(warningTelegraph);

        // ¡Erupción de Raíz punzante real!
        CameraFollow2D.Shake(0.2f, 0.25f);
        KiwiAudio.PlayBossRoar();

        GameObject rootSpike = new GameObject("RaizPunzante");
        rootSpike.tag = "Hazard";
        rootSpike.transform.position = new Vector3(targetPos.x, targetPos.y + 0.3f, 0f);
        rootSpike.transform.localScale = new Vector3(1.6f, 1.8f, 1f);

        SpriteRenderer rSr = rootSpike.AddComponent<SpriteRenderer>();
        rSr.sprite = spikeSprite != null ? spikeSprite : thornSprite;
        rSr.color = Color.white;
        rSr.sortingOrder = 4;

        BoxCollider2D bc = rootSpike.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(0.9f, 1.6f);

        rootSpike.AddComponent<SpikeHazard>().Setup(1.2f);

        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }

    // Ataque 2: Ráfaga de Astillas y Espinas de Madera (utiliza ThornProjectile.png)
    private void ShootThornBarrage()
    {
        if (playerTransform == null) return;
        KiwiAudio.PlayFlyPop();

        Vector2 baseDir = (playerTransform.position - transform.position).normalized;
        for (int i = -1; i <= 1; i++)
        {
            Vector2 spreadDir = Quaternion.Euler(0, 0, i * 15f) * baseDir;
            SpawnThornProjectile(transform.position + Vector3.up * 0.5f, spreadDir, 8.5f);
        }
    }

    // Ataque 3: Lluvia de Esporas desde el dosel
    private void ShootSporeRain()
    {
        KiwiAudio.PlayEnemyHit();
        for (int i = -2; i <= 2; i++)
        {
            Vector3 dropPos = new Vector3(playerTransform.position.x + i * 2.0f, transform.position.y + 5.5f, 0f);
            SpawnThornProjectile(dropPos, Vector2.down, 6.5f);
        }
    }

    private void SpawnThornProjectile(Vector3 pos, Vector2 dir, float spd)
    {
        GameObject proj = new GameObject("AstillaMadera");
        proj.tag = "Hazard";
        proj.transform.position = pos;
        proj.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        SpriteRenderer pSr = proj.AddComponent<SpriteRenderer>();
        pSr.sprite = thornSprite != null ? thornSprite : projectileSprite;
        pSr.color = Color.white;
        pSr.sortingOrder = 5;

        // Rotar hacia la dirección de vuelo
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);

        CircleCollider2D cc = proj.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.28f;

        proj.AddComponent<BossProjectile>().Setup(dir, spd);
    }

    // Fase de Furia: Terremoto y Ondas de Choque (utiliza EarthquakeShockwave.png)
    private IEnumerator EarthquakeSlamAttack()
    {
        isAttacking = true;
        KiwiAudio.PlayBossRoar();
        CameraFollow2D.Shake(0.4f, 0.45f);

        Vector3 startP = transform.position;
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            transform.position = startP + Vector3.up * Mathf.Sin((elapsed / 0.3f) * Mathf.PI) * 1.5f;
            yield return null;
        }
        transform.position = startP;

        // Ondas de choque en el suelo con sprite propio
        SpawnShockwave(transform.position + Vector3.down * 1.2f, Vector2.left);
        SpawnShockwave(transform.position + Vector3.down * 1.2f, Vector2.right);

        yield return new WaitForSeconds(0.6f);
        isAttacking = false;
    }

    private void SpawnShockwave(Vector3 pos, Vector2 dir)
    {
        GameObject wave = new GameObject("OndaChoqueTierra");
        wave.tag = "Hazard";
        wave.transform.position = pos;
        wave.transform.localScale = new Vector3(1.4f, 1.2f, 1f);

        SpriteRenderer wSr = wave.AddComponent<SpriteRenderer>();
        wSr.sprite = shockwaveSprite != null ? shockwaveSprite : thornSprite;
        wSr.color = Color.white;
        wSr.sortingOrder = 4;
        if (dir.x < 0) wSr.flipX = true;

        BoxCollider2D bc = wave.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(1.2f, 0.7f);

        wave.AddComponent<BossProjectile>().Setup(dir, 7.5f);
    }

    public void ApplyElementalHit(ElementType element, int damage, Vector2 hitDirection)
    {
        if (isDead) return;

        int finalDmg = damage;

        // 🔥 ¡DEBILIDAD AL FUEGO!
        if (element == ElementType.Fuego)
        {
            finalDmg *= 2;
            StartCoroutine(BurnFlash());
            KiwiAudio.PlayFlyPop();
        }
        else
        {
            StartCoroutine(HurtFlash());
        }

        currentHealth -= finalDmg;
        KiwiAudio.PlayEnemyHit();

        if (!isRaging && currentHealth <= maxHealth / 2)
        {
            isRaging = true;
            KiwiAudio.PlayBossRoar();
            CameraFollow2D.Shake(0.5f, 0.5f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(int damageAmount, Vector2 hitDirection)
    {
        ApplyElementalHit(ElementType.Fuego, damageAmount, hitDirection);
    }

    private IEnumerator BurnFlash()
    {
        if (sr != null) sr.color = new Color(1f, 0.4f, 0.1f);
        yield return new WaitForSeconds(0.18f);
        if (sr != null && !isDead) sr.color = isRaging ? new Color(1f, 0.3f, 0.3f) : originalColor;
    }

    private IEnumerator HurtFlash()
    {
        if (sr != null) sr.color = Color.white;
        yield return new WaitForSeconds(0.12f);
        if (sr != null && !isDead) sr.color = isRaging ? new Color(1f, 0.3f, 0.3f) : originalColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        DaggerProjectile dagger = collision.GetComponent<DaggerProjectile>();
        if (dagger != null)
        {
            ApplyElementalHit(dagger.Element, 1, Vector2.right);
            Destroy(dagger.gameObject);
            return;
        }

        if (collision.CompareTag("Player"))
        {
            KiwiController kiwi = collision.GetComponent<KiwiController>();
            if (kiwi != null && collision.transform.position.y > transform.position.y + 1.2f)
            {
                kiwi.StompBounce(16f);
                TakeDamage(1, Vector2.up);
                return;
            }

            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }

    private void Die()
    {
        isDead = true;
        if (col != null) col.enabled = false;

        KiwiAudio.PlayVictory();
        CameraFollow2D.Shake(0.5f, 0.6f);
        StartCoroutine(PurificationSequence());
    }

    private IEnumerator PurificationSequence()
    {
        float timer = 0f;
        float duration = 2.2f;
        Color pureNatureColor = new Color(0.2f, 1f, 0.4f, 0.6f);

        if (rewardSeedSprite != null)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject seed = new GameObject("Semilla Naturaleza");
                seed.transform.position = transform.position + new Vector3((i - 1.5f) * 1.3f, 1.2f, 0);
                seed.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

                SpriteRenderer sSr = seed.AddComponent<SpriteRenderer>();
                sSr.sprite = rewardSeedSprite;

                CircleCollider2D cc = seed.AddComponent<CircleCollider2D>();
                cc.isTrigger = true;
                cc.radius = 0.35f;

                seed.AddComponent<EnergySeedPickup>();
            }
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            if (sr != null) sr.color = Color.Lerp(originalColor, pureNatureColor, t);
            transform.position += Vector3.up * 1.3f * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnGUI()
    {
        if (isDead || playerTransform == null) return;
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist > 18f) return;

        float screenW = Screen.width;
        float barWidth = Mathf.Min(420f, screenW * 0.65f);
        float barHeight = 20f;
        float barX = (screenW - barWidth) / 2f;
        float barY = 30f;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 15;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = isRaging ? new Color(1f, 0.3f, 0.3f) : new Color(0.4f, 1f, 0.4f);
        string bossTitle = isRaging ? "⚠ EL GUARDIÁN PÚTRIDO - [FURIA DESATADA] ⚠" : "EL GUARDIÁN PÚTRIDO (ESPÍRITU DEL BOSQUE)";
        GUI.Label(new Rect(barX - 80, barY - 26, barWidth + 160, 24), bossTitle, titleStyle);

        GUI.Box(new Rect(barX - 3, barY - 3, barWidth + 6, barHeight + 6), GUIContent.none);

        float healthPct = Mathf.Clamp01((float)currentHealth / maxHealth);
        Color prevColor = GUI.color;
        GUI.color = isRaging ? Color.Lerp(Color.red, Color.yellow, healthPct) : Color.Lerp(Color.red, new Color(0.2f, 0.95f, 0.35f), healthPct);
        GUI.DrawTexture(new Rect(barX, barY, barWidth * healthPct, barHeight), Texture2D.whiteTexture);
        GUI.color = prevColor;

        GUIStyle hpStyle = new GUIStyle(GUI.skin.label);
        hpStyle.alignment = TextAnchor.MiddleCenter;
        hpStyle.fontSize = 12;
        hpStyle.fontStyle = FontStyle.Bold;
        hpStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(barX, barY - 1, barWidth, barHeight), $"{currentHealth} / {maxHealth}", hpStyle);
    }
}
