using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Jefe Regional del Mar de Plástico: La Ballena de Plástico (Espíritu de Agua Corrompido).
/// Combate en 2 fases con ataques a distancia y embestidas en fase de furia.
/// Al purificarse, restaura el océano y libera el Espíritu del Agua.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlasticWhaleBossAI : MonoBehaviour, IDamageable
{
    [Header("Estadísticas del Jefe")]
    [SerializeField] private int maxHealth = 25;
    [SerializeField] private float swimSpeed = 3.2f;
    [SerializeField] private float rageSwimSpeed = 5.5f;
    [SerializeField] private float arenaMinX = 36f;
    [SerializeField] private float arenaMaxX = 49f;

    [Header("Ataques")]
    [SerializeField] private float shootInterval = 3.0f;
    [SerializeField] private float rageShootInterval = 1.8f;
    [SerializeField] private Sprite projectileSprite;
    [SerializeField] private Sprite rewardSeedSprite;

    private int currentHealth;
    private bool isRaging = false;
    private bool isDead = false;
    private bool isCharging = false;
    private float nextShootTime = 0f;
    private float nextChargeTime = 0f;
    private int moveDirection = -1;

    private Transform playerTransform;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private Color originalColor;
    private Vector3 basePosition;

    public static PlasticWhaleBossAI ActiveBoss { get; private set; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

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
        basePosition = transform.position;
        ActiveBoss = this;
    }

    private void Start()
    {
        FindPlayer();
        nextShootTime = Time.time + 2f;
        nextChargeTime = Time.time + 6f;
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

        // Comprobar si Kiwi está dentro o cerca de la arena del jefe
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distToPlayer > 18f) return; // Esperar a que Kiwi se aproxime

        // 1. Movimiento oscilatorio de nado de la Ballena
        if (!isCharging)
        {
            float speed = isRaging ? rageSwimSpeed : swimSpeed;
            transform.Translate(Vector3.right * moveDirection * speed * Time.deltaTime, Space.World);

            // Flotar arriba y abajo suavemente
            float newY = basePosition.y + Mathf.Sin(Time.time * 2.2f) * 0.8f;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Cambiar de dirección al tocar los extremos de la arena
            if (transform.position.x <= arenaMinX)
            {
                moveDirection = 1;
                if (sr != null) sr.flipX = true;
            }
            else if (transform.position.x >= arenaMaxX)
            {
                moveDirection = -1;
                if (sr != null) sr.flipX = false;
            }
        }

        // 2. Ataque a distancia: Alternar entre disparo directo, Tsunami terrestre y lluvia aérea
        float currentShootInterval = isRaging ? rageShootInterval : shootInterval;
        if (Time.time >= nextShootTime && !isCharging)
        {
            int attackType = Random.Range(0, 3);
            if (attackType == 0) ShootToxicTrash();
            else if (attackType == 1) ShootGroundTsunami();
            else ShootTrashRain();

            nextShootTime = Time.time + currentShootInterval;
        }

        // 3. Fase de Furia: Embestida veloz (Charge) con temblor de cámara
        if (isRaging && Time.time >= nextChargeTime && !isCharging)
        {
            StartCoroutine(ChargeAttack());
            nextChargeTime = Time.time + 5.0f;
        }
    }

    private void ShootToxicTrash()
    {
        if (playerTransform == null) return;

        SpawnProjectile(transform.position, (playerTransform.position - transform.position).normalized, 8f);
    }

    private void ShootGroundTsunami()
    {
        // Ola tóxica a ras de suelo que cruza la arena (obliga a usar el Sobre-Salto)
        CameraFollow2D.Shake(0.2f, 0.25f);
        KiwiAudio.PlayBossRoar();

        Vector3 spawnPos = new Vector3(transform.position.x, -1.0f, 0f);
        Vector2 dir = (playerTransform.position.x > transform.position.x) ? Vector2.right : Vector2.left;

        GameObject wave = new GameObject("OlaTsunami");
        wave.tag = "Hazard";
        wave.transform.position = spawnPos;
        wave.transform.localScale = new Vector3(2.0f, 0.9f, 1f);

        SpriteRenderer wSr = wave.AddComponent<SpriteRenderer>();
        wSr.sprite = projectileSprite != null ? projectileSprite : sr.sprite;
        wSr.color = new Color(0.1f, 0.7f, 0.9f, 0.9f); // Azul tóxico oleoso
        wSr.sortingOrder = 4;

        BoxCollider2D c = wave.AddComponent<BoxCollider2D>();
        c.isTrigger = true;
        c.size = new Vector2(1.8f, 0.8f);

        wave.AddComponent<BossProjectile>().Setup(dir, 9.5f);
    }

    private void ShootTrashRain()
    {
        // Lluvia de 3 proyectiles cayendo desde arriba
        KiwiAudio.PlayEnemyHit();
        for (int i = -1; i <= 1; i++)
        {
            Vector3 dropPos = new Vector3(playerTransform.position.x + i * 2.5f, 6.5f, 0f);
            SpawnProjectile(dropPos, Vector2.down, 7f);
        }
    }

    private void SpawnProjectile(Vector3 pos, Vector2 dir, float spd)
    {
        GameObject proj = new GameObject("ProyectilResiduo");
        proj.tag = "Hazard";
        proj.transform.position = pos;
        proj.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        SpriteRenderer pSr = proj.AddComponent<SpriteRenderer>();
        pSr.sprite = projectileSprite != null ? projectileSprite : (rewardSeedSprite != null ? rewardSeedSprite : sr.sprite);
        pSr.color = new Color(0.7f, 0.2f, 0.9f);
        pSr.sortingOrder = 3;

        CircleCollider2D c = proj.AddComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius = 0.4f;

        Rigidbody2D projRb = proj.AddComponent<Rigidbody2D>();
        projRb.bodyType = RigidbodyType2D.Kinematic;
        projRb.useFullKinematicContacts = true;

        proj.AddComponent<BossProjectile>().Setup(dir, spd);
    }

    private IEnumerator ChargeAttack()
    {
        isCharging = true;
        KiwiAudio.PlayBossRoar();
        CameraFollow2D.Shake(0.35f, 0.4f);

        // Carga preparatoria: parpadeo rojo
        if (sr != null) sr.color = Color.red;
        yield return new WaitForSeconds(0.45f);

        // Embestida rápida hacia la posición de Kiwi
        float chargeDir = (playerTransform.position.x > transform.position.x) ? 1f : -1f;
        if (sr != null) sr.flipX = (chargeDir > 0);

        float chargeDuration = 0.9f;
        float timer = 0f;
        while (timer < chargeDuration)
        {
            timer += Time.deltaTime;
            transform.Translate(Vector3.right * chargeDir * 12.5f * Time.deltaTime, Space.World);

            // Delimitar a la arena
            float clampedX = Mathf.Clamp(transform.position.x, arenaMinX - 1f, arenaMaxX + 1f);
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
            yield return null;
        }

        CameraFollow2D.Shake(0.4f, 0.3f); // Impacto contra la pared
        if (sr != null) sr.color = isRaging ? new Color(1f, 0.4f, 0.4f) : originalColor;
        isCharging = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        // Daga de Kiwi
        DaggerProjectile dagger = collision.GetComponent<DaggerProjectile>();
        if (dagger != null)
        {
            TakeDamage(1, Vector2.right);
            Destroy(dagger.gameObject);
            return;
        }

        // Choque con Kiwi
        if (collision.CompareTag("Player"))
        {
            KiwiController kiwi = collision.GetComponent<KiwiController>();
            // Si Kiwi pisa la espalda de la Ballena desde arriba
            if (kiwi != null && collision.transform.position.y > transform.position.y + 0.8f)
            {
                kiwi.StompBounce(16f);
                TakeDamage(1, Vector2.up);
                return;
            }

            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }

    public void TakeDamage(int damageAmount, Vector2 hitDirection)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        KiwiAudio.PlayEnemyHit();

        // Entrar en fase de furia al 50% de salud
        if (currentHealth <= maxHealth / 2 && !isRaging)
        {
            isRaging = true;
            KiwiAudio.PlayBossRoar();
            if (sr != null) sr.color = new Color(1f, 0.45f, 0.45f);
        }

        StartCoroutine(HurtFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HurtFlash()
    {
        if (sr != null) sr.color = Color.white;
        yield return new WaitForSeconds(0.12f);
        if (sr != null) sr.color = isRaging ? new Color(1f, 0.45f, 0.45f) : originalColor;
    }

    private void Die()
    {
        isDead = true;
        if (col != null) col.enabled = false;

        KiwiAudio.PlayVictory();

        // Secuencia de purificación: La ballena se vuelve pacífica y transparente,
        // restaurando el agua y soltando una lluvia de semillas de energía
        StartCoroutine(PurificationSequence());
    }

    private IEnumerator PurificationSequence()
    {
        float timer = 0f;
        float duration = 2.0f;
        Color startColor = sr != null ? sr.color : Color.white;
        Color pureWaterColor = new Color(0.2f, 0.9f, 1f, 0.5f); // Resplandor divino del Espíritu del Agua

        // Soltar 4 semillas de energía como recompensa
        if (rewardSeedSprite != null)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject seed = new GameObject("Semilla Victoria");
                seed.transform.position = transform.position + new Vector3((i - 1.5f) * 1.2f, 1.2f, 0);
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

            if (sr != null)
            {
                sr.color = Color.Lerp(startColor, pureWaterColor, t);
            }
            // Ascender majestuosamente hacia el cielo
            transform.position += Vector3.up * 1.5f * Time.deltaTime;
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

        // Título del Jefe con cambio de color en Fase de Furia
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 15;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = isRaging ? new Color(1f, 0.35f, 0.35f) : new Color(0.35f, 0.9f, 1f);
        string bossTitle = isRaging ? "⚠ LEVIATÁN DE PLÁSTICO - [FURIA DESATADA] ⚠" : "LEVIATÁN DE PLÁSTICO (ESPÍRITU DEL OCÉANO)";
        GUI.Label(new Rect(barX - 80, barY - 26, barWidth + 160, 24), bossTitle, titleStyle);

        // Marco y Fondo
        GUI.Box(new Rect(barX - 3, barY - 3, barWidth + 6, barHeight + 6), GUIContent.none);

        // Barra de relleno con gradiente dinámico
        float healthPct = Mathf.Clamp01((float)currentHealth / maxHealth);
        Color prevColor = GUI.color;
        GUI.color = isRaging ? Color.Lerp(Color.red, Color.yellow, healthPct) : Color.Lerp(Color.red, new Color(0.1f, 0.85f, 1f), healthPct);
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

    public void Setup(Sprite projSprite, Sprite seedSprite)
    {
        projectileSprite = projSprite;
        rewardSeedSprite = seedSprite;
    }
}

/// <summary>
/// Proyectil de residuo lanzado por La Ballena de Plástico.
/// </summary>
public class BossProjectile : MonoBehaviour
{
    private Vector2 moveDir;
    private float moveSpeed;

    public void Setup(Vector2 dir, float speed)
    {
        moveDir = dir;
        moveSpeed = speed;
        Destroy(gameObject, 4.0f);
    }

    private void Update()
    {
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
