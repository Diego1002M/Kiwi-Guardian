using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Jefe Regional del Desierto de Residuos: El Superenjambre de la Génesis Tóxica (Espíritu de Fuego/Tierra Corrompido).
/// Masa palpitante de biomatriz tóxica, parásitos y pústulas ácidas.
/// Combate en 2 fases con andanadas ácidas, estallidos radiales de bilis y metamorfosis furiosa.
/// DEBILIDAD CRÍTICA AL AGUA: Las dagas de agua [3 / E] neutralizan el ácido y causan daño doble (2x).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ToxicHiveBossAI : MonoBehaviour, IDamageable, IElementalTarget
{
    [Header("Estadísticas del Jefe")]
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private float moveSpeed = 2.4f;
    [SerializeField] private float rageMoveSpeed = 4.0f;
    [SerializeField] private float arenaMinX = 51f;
    [SerializeField] private float arenaMaxX = 65f;

    [Header("Ataques")]
    [SerializeField] private float attackInterval = 3.2f;
    [SerializeField] private float rageAttackInterval = 1.8f;
    [SerializeField] private Sprite fireballSprite;
    [SerializeField] private Sprite rewardSeedSprite;

    private int currentHealth;
    private bool isRaging = false;
    private bool isDead = false;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;
    private float nextDelugeTime = 0f;
    private int moveDir = 1;

    private Transform playerTransform;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private Color originalColor;
    private Vector3 baseScale;
    private Sprite acidSprite;
    private Sprite pustuleSprite;

    public static ToxicHiveBossAI ActiveBoss { get; private set; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public ElementType TargetElement => ElementType.Fuego;

    public void SetupArena(float minX, float maxX, Sprite fireball, Sprite reward)
    {
        arenaMinX = minX;
        arenaMaxX = maxX;
        fireballSprite = fireball;
        rewardSeedSprite = reward;
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        baseScale = transform.localScale;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.gravityScale = 0f;

        currentHealth = maxHealth;
        ActiveBoss = this;
        acidSprite = Resources.Load<Sprite>("Sprites/AcidGlob".Replace(""", ""));
        pustuleSprite = Resources.Load<Sprite>("Sprites/ThornProjectile".Replace(""", ""));
        if (rewardSeedSprite == null) rewardSeedSprite = Resources.Load<Sprite>("Sprites/EnergySeed".Replace(""", ""));
    }

    private void Start()
    {
        FindPlayer();
        nextAttackTime = Time.time + 2.0f;
        nextDelugeTime = Time.time + 5.0f;
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

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist > 18f) return;

        // 1. Pulsación orgánica palpitante de la colmena
        float pulseSpeed = isRaging ? 6.5f : 3.0f;
        float pulseAmount = isRaging ? 0.08f : 0.04f;
        float scaleMod = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = new Vector3(baseScale.x * scaleMod, baseScale.y * (2f - scaleMod), baseScale.z);

        // 2. Desplazamiento reptante por la arena
        if (!isAttacking)
        {
            float speed = isRaging ? rageMoveSpeed : moveSpeed;
            transform.Translate(Vector3.right * moveDir * speed * Time.deltaTime, Space.World);

            if (transform.position.x <= arenaMinX)
            {
                moveDir = 1;
                if (sr != null) sr.flipX = false;
            }
            else if (transform.position.x >= arenaMaxX)
            {
                moveDir = -1;
                if (sr != null) sr.flipX = true;
            }
        }

        // 3. Patrones de Ataque
        float currentInterval = isRaging ? rageAttackInterval : attackInterval;
        if (Time.time >= nextAttackTime && !isAttacking)
        {
            int atk = Random.Range(0, 3);
            if (atk == 0) StartCoroutine(AcidSludgeBarrage());
            else if (atk == 1) RadialPustuleBurst();
            else SpawnSwarmMinion();

            nextAttackTime = Time.time + currentInterval;
        }

        // 4. Fase de Furia: Diluvio de lluvia ácida
        if (isRaging && Time.time >= nextDelugeTime && !isAttacking)
        {
            StartCoroutine(AcidRainDeluge());
            nextDelugeTime = Time.time + 5.5f;
        }
    }

    // Ataque 1: Andanada de Flemas Ácidas en parábola
    private IEnumerator AcidSludgeBarrage()
    {
        isAttacking = true;
        KiwiAudio.PlayBossRoar();
        CameraFollow2D.Shake(0.2f, 0.2f);

        int count = isRaging ? 3 : 2;
        for (int i = 0; i < count; i++)
        {
            if (playerTransform == null) break;

            Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
            Vector2 target = (Vector2)playerTransform.position + new Vector2((i - 0.5f) * 2.2f, 0f);
            Vector2 launchVel = CalculateParabolicVelocity(spawnPos, target, 0.95f);

            GameObject glob = new GameObject("FlemaAcida");
            glob.tag = "Hazard";
            glob.transform.position = spawnPos;
            glob.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            SpriteRenderer gSr = glob.AddComponent<SpriteRenderer>();
            gSr.sprite = acidSprite != null ? acidSprite : fireballSprite;
            gSr.color = Color.white; // Verde radioactivo
            gSr.sortingOrder = 5;

            CircleCollider2D cc = glob.AddComponent<CircleCollider2D>();
            cc.isTrigger = true;
            cc.radius = 0.35f;

            Rigidbody2D gRb = glob.AddComponent<Rigidbody2D>();
            gRb.gravityScale = 1.5f;
            gRb.linearVelocity = launchVel;

            glob.AddComponent<AcidGlobImpact>();

            yield return new WaitForSeconds(0.3f);
        }

        isAttacking = false;
    }

    private Vector2 CalculateParabolicVelocity(Vector2 origin, Vector2 target, float timeToTarget)
    {
        float vx = (target.x - origin.x) / timeToTarget;
        float vy = (target.y - origin.y) / timeToTarget + 0.5f * Mathf.Abs(Physics2D.gravity.y * 1.5f) * timeToTarget;
        return new Vector2(vx, vy);
    }

    // Ataque 2: Estallido Radial de Pústulas (8 proyectiles en estrella)
    private void RadialPustuleBurst()
    {
        KiwiAudio.PlayFlyPop();
        CameraFollow2D.Shake(0.25f, 0.25f);

        int numProj = 8;
        for (int i = 0; i < numProj; i++)
        {
            float angle = i * (360f / numProj);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject needle = new GameObject("PustulaEspora");
            needle.tag = "Hazard";
            needle.transform.position = transform.position + (Vector3)dir * 1.0f;
            needle.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

            SpriteRenderer nSr = needle.AddComponent<SpriteRenderer>();
            nSr.sprite = pustuleSprite != null ? pustuleSprite : acidSprite;
            nSr.color = Color.white; // Amarillo bilis
            nSr.sortingOrder = 5;

            CircleCollider2D cc = needle.AddComponent<CircleCollider2D>();
            cc.isTrigger = true;
            cc.radius = 0.25f;

            needle.AddComponent<BossProjectile>().Setup(dir, 7.5f);
        }
    }

    // Ataque 3: Invocación de Larva del Enjambre
    private void SpawnSwarmMinion()
    {
        KiwiAudio.PlayEnemyHit();

        GameObject larva = new GameObject("LarvaEnjambre");
        larva.tag = "Hazard";
        larva.transform.position = transform.position + new Vector3(moveDir * 1.5f, 0.3f, 0f);
        larva.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

        SpriteRenderer lSr = larva.AddComponent<SpriteRenderer>();
        lSr.sprite = acidSprite != null ? acidSprite : fireballSprite;
        lSr.color = new Color(0.9f, 0.85f, 0.7f, 1f); // Tono larva beige
        lSr.sortingOrder = 4;

        CircleCollider2D cc = larva.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.3f;

        Vector2 crawlDir = (playerTransform != null && playerTransform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
        larva.AddComponent<BossProjectile>().Setup(crawlDir, 5.0f);
    }

    // Fase de Furia: Diluvio de Lluvia Ácida desde las alturas
    private IEnumerator AcidRainDeluge()
    {
        isAttacking = true;
        KiwiAudio.PlayBossRoar();
        CameraFollow2D.Shake(0.35f, 0.4f);

        for (int i = -3; i <= 3; i++)
        {
            Vector3 dropPos = new Vector3(playerTransform.position.x + i * 1.8f, transform.position.y + 6.5f, 0f);
            
            GameObject drop = new GameObject("GotaAcida");
            drop.tag = "Hazard";
            drop.transform.position = dropPos;
            drop.transform.localScale = new Vector3(0.55f, 0.8f, 1f);

            SpriteRenderer dSr = drop.AddComponent<SpriteRenderer>();
            dSr.sprite = acidSprite != null ? acidSprite : fireballSprite;
            dSr.color = Color.white;
            dSr.sortingOrder = 5;

            CircleCollider2D cc = drop.AddComponent<CircleCollider2D>();
            cc.isTrigger = true;
            cc.radius = 0.25f;

            drop.AddComponent<BossProjectile>().Setup(Vector2.down, 8.5f);
            yield return new WaitForSeconds(0.12f);
        }

        isAttacking = false;
    }

    public void ApplyElementalHit(ElementType element, int damage, Vector2 hitDirection)
    {
        if (isDead) return;

        int finalDmg = damage;

        // 💧 ¡DEBILIDAD AL AGUA!
        if (element == ElementType.Agua)
        {
            finalDmg *= 2;
            StartCoroutine(WaterFlash());
            KiwiAudio.PlayFlyPop();
        }
        else
        {
            StartCoroutine(HurtFlash());
        }

        currentHealth -= finalDmg;
        KiwiAudio.PlayEnemyHit();

        // Metamorfosis Furiosa (<50% vida)
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
        ApplyElementalHit(ElementType.Agua, damageAmount, hitDirection);
    }

    private IEnumerator WaterFlash()
    {
        if (sr != null) sr.color = new Color(0.2f, 0.9f, 1f); // Enfriamiento/dilución acuosa
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
            if (kiwi != null && collision.transform.position.y > transform.position.y + 1.0f)
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
        StartCoroutine(PurifySequence());
    }

    private IEnumerator PurifySequence()
    {
        float timer = 0f;
        float duration = 2.2f;
        Color pureAmberColor = new Color(1f, 0.85f, 0.25f, 0.6f); // Cristal ámbar purificado de la tierra

        if (rewardSeedSprite != null)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject seed = new GameObject("Semilla Tierra");
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
            if (sr != null) sr.color = Color.Lerp(originalColor, pureAmberColor, t);
            transform.position += Vector3.up * 1.2f * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnGUI()
    {
        if (isDead || playerTransform == null) return;
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist > 18f) return;

        // Barra de Vida Épica del Jefe en la parte superior
        float screenW = Screen.width;
        float barWidth = Mathf.Min(420f, screenW * 0.65f);
        float barHeight = 20f;
        float barX = (screenW - barWidth) / 2f;
        float barY = 30f;

        // Título del Jefe
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 15;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = isRaging ? new Color(1f, 0.3f, 0.8f) : new Color(0.6f, 1f, 0.2f);
        string bossTitle = isRaging ? "⚠ SUPERENJAMBRE TÓXICO - [METAMORFOSIS FURIOSA] ⚠" : "EL SUPERENJAMBRE DE LA GÉNESIS TÓXICA (ESPÍRITU DE LA TIERRA)";
        GUI.Label(new Rect(barX - 80, barY - 26, barWidth + 160, 24), bossTitle, titleStyle);

        // Marco y Fondo
        GUI.Box(new Rect(barX - 3, barY - 3, barWidth + 6, barHeight + 6), GUIContent.none);

        // Barra de relleno con gradiente dinámico
        float healthPct = Mathf.Clamp01((float)currentHealth / maxHealth);
        Color prevColor = GUI.color;
        GUI.color = isRaging ? Color.Lerp(new Color(0.9f, 0.1f, 0.5f), Color.yellow, healthPct) : Color.Lerp(Color.red, new Color(0.4f, 1f, 0.2f), healthPct);
        GUI.DrawTexture(new Rect(barX, barY, barWidth * healthPct, barHeight), Texture2D.whiteTexture);
        GUI.color = prevColor;

        // Texto numérico de salud
        GUIStyle hpStyle = new GUIStyle(GUI.skin.label);
        hpStyle.alignment = TextAnchor.MiddleCenter;
        hpStyle.fontSize = 12;
        hpStyle.fontStyle = FontStyle.Bold;
        hpStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(barX, barY - 1, barWidth, barHeight), $"{currentHealth} / {maxHealth}", hpStyle);
    }
}

/// <summary>
/// Flema ácida que detona al impactar contra el suelo o el jugador.
/// </summary>
public class AcidGlobImpact : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 4.0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
            Explode();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        KiwiAudio.PlayFlyPop();
        CameraFollow2D.Shake(0.12f, 0.15f);
        Destroy(gameObject);
    }
}
