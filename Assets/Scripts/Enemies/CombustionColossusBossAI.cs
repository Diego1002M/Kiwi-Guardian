using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Jefe Regional de Ciudad de Humo: El Coloso de la Combustión Infame (Espíritu del Aire Corrompido).
/// Combate de 2 fases con morteros de smog, llamaradas de vapor y modo sobrecalentamiento.
/// DEBILIDAD CRÍTICA AL AGUA: Las dagas de agua [3 / E] apagan sus calderas y causan daño doble (2x).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CombustionColossusBossAI : MonoBehaviour, IDamageable, IElementalTarget
{
    [Header("Estadísticas del Jefe")]
    [SerializeField] private int maxHealth = 28;
    [SerializeField] private float walkSpeed = 2.6f;
    [SerializeField] private float rageWalkSpeed = 4.8f;
    [SerializeField] private float arenaMinX = 18f;
    [SerializeField] private float arenaMaxX = 33f;

    [Header("Ataques")]
    [SerializeField] private float attackInterval = 3.2f;
    [SerializeField] private float rageAttackInterval = 1.9f;
    [SerializeField] private Sprite projectileSprite;
    [SerializeField] private Sprite rewardSeedSprite;

    private int currentHealth;
    private bool isRaging = false;
    private bool isDead = false;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;
    private float nextChargeTime = 0f;
    private int moveDirection = 1;

    private Transform playerTransform;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private Color originalColor;

    // Sprites dedicados de ataques (evita clonar al jefe)
    private Sprite smogBombSprite;
    private Sprite steamJetSprite;
    private Sprite gearSprite;

    public static CombustionColossusBossAI ActiveBoss { get; private set; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public ElementType TargetElement => ElementType.Aire;

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

        // Cargar sprites dedicados
        smogBombSprite = Resources.Load<Sprite>("Sprites/SmogBomb");
        steamJetSprite = Resources.Load<Sprite>("Sprites/SteamFlameJet");
        gearSprite = Resources.Load<Sprite>("Sprites/GearProjectile");
        if (rewardSeedSprite == null) rewardSeedSprite = Resources.Load<Sprite>("Sprites/EnergySeed");
    }

    private void Start()
    {
        FindPlayer();
        nextAttackTime = Time.time + 2.0f;
        nextChargeTime = Time.time + 6.0f;
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

        // 1. Patrulla del Coloso
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

        // 2. Patrones de Ataque
        float currentInterval = isRaging ? rageAttackInterval : attackInterval;
        if (Time.time >= nextAttackTime && !isAttacking)
        {
            int atk = Random.Range(0, 3);
            if (atk == 0) StartCoroutine(SmogMortarBarrage());
            else if (atk == 1) StartCoroutine(SteamJetBlast());
            else ShootGearProjectile();

            nextAttackTime = Time.time + currentInterval;
        }

        // 3. Fase de Furia: Embestida veloz a vapor
        if (isRaging && Time.time >= nextChargeTime && !isAttacking)
        {
            StartCoroutine(OverheatChargeAttack());
            nextChargeTime = Time.time + 5.0f;
        }
    }

    // Ataque 1: Mortero de Smog disparado desde las chimeneas (utiliza SmogBomb.png)
    private IEnumerator SmogMortarBarrage()
    {
        isAttacking = true;
        KiwiAudio.PlayBossRoar();
        CameraFollow2D.Shake(0.2f, 0.25f);

        int count = isRaging ? 3 : 2;
        for (int i = 0; i < count; i++)
        {
            if (playerTransform == null) break;

            Vector3 spawnPos = transform.position + Vector3.up * 1.8f;
            Vector2 target = (Vector2)playerTransform.position + new Vector2((i - 0.5f) * 2.0f, 0f);
            Vector2 launchVelocity = CalculateParabolicVelocity(spawnPos, target, 1.0f);

            GameObject mortar = new GameObject("BombaHumo");
            mortar.tag = "Hazard";
            mortar.transform.position = spawnPos;
            mortar.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

            SpriteRenderer mSr = mortar.AddComponent<SpriteRenderer>();
            mSr.sprite = smogBombSprite != null ? smogBombSprite : projectileSprite;
            mSr.color = Color.white;
            mSr.sortingOrder = 5;

            CircleCollider2D cc = mortar.AddComponent<CircleCollider2D>();
            cc.isTrigger = true;
            cc.radius = 0.35f;

            Rigidbody2D mRb = mortar.AddComponent<Rigidbody2D>();
            mRb.gravityScale = 1.6f;
            mRb.linearVelocity = launchVelocity;

            mortar.AddComponent<SmogBombExplosion>();

            yield return new WaitForSeconds(0.35f);
        }

        isAttacking = false;
    }

    private Vector2 CalculateParabolicVelocity(Vector2 origin, Vector2 target, float timeToTarget)
    {
        float vx = (target.x - origin.x) / timeToTarget;
        float vy = (target.y - origin.y) / timeToTarget + 0.5f * Mathf.Abs(Physics2D.gravity.y * 1.6f) * timeToTarget;
        return new Vector2(vx, vy);
    }

    // Ataque 2: Chorro de Fuego/Vapor Industrial (utiliza SteamFlameJet.png)
    private IEnumerator SteamJetBlast()
    {
        isAttacking = true;
        KiwiAudio.PlayEnemyHit();

        // Breve aviso en el jefe
        if (sr != null) sr.color = new Color(1f, 0.6f, 0.2f);
        yield return new WaitForSeconds(0.45f);
        if (sr != null && !isDead) sr.color = isRaging ? new Color(1f, 0.3f, 0.3f) : originalColor;

        KiwiAudio.PlayBossRoar();
        CameraFollow2D.Shake(0.3f, 0.35f);

        Vector2 dir = (playerTransform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
        Vector3 blastSpawn = transform.position + (Vector3)dir * 1.5f + Vector3.up * 0.2f;

        GameObject jet = new GameObject("ChorroVaporArdiente");
        jet.tag = "Hazard";
        jet.transform.position = blastSpawn;
        jet.transform.localScale = new Vector3(1.8f, 1.4f, 1f);

        SpriteRenderer jSr = jet.AddComponent<SpriteRenderer>();
        jSr.sprite = steamJetSprite != null ? steamJetSprite : smogBombSprite;
        jSr.color = Color.white;
        jSr.sortingOrder = 4;
        if (dir.x < 0) jSr.flipX = true;

        BoxCollider2D bc = jet.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(2.2f, 1.0f);

        jet.AddComponent<BossProjectile>().Setup(dir, 10.5f);

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    // Ataque 3: Engranaje Dentado Rebotador (utiliza GearProjectile.png)
    private void ShootGearProjectile()
    {
        if (playerTransform == null) return;
        KiwiAudio.PlayFlyPop();

        Vector2 dir = (playerTransform.position - transform.position).normalized;
        GameObject gear = new GameObject("EngranajeRebotador");
        gear.tag = "Hazard";
        gear.transform.position = transform.position + Vector3.up * 0.4f;
        gear.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        SpriteRenderer gSr = gear.AddComponent<SpriteRenderer>();
        gSr.sprite = gearSprite != null ? gearSprite : smogBombSprite;
        gSr.color = Color.white;
        gSr.sortingOrder = 5;

        CircleCollider2D cc = gear.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.35f;

        gear.AddComponent<BossProjectile>().Setup(dir, 9.0f);
    }

    // Fase de Furia: Embestida por Sobrecalentamiento
    private IEnumerator OverheatChargeAttack()
    {
        isAttacking = true;
        KiwiAudio.PlayBossRoar();
        CameraFollow2D.Shake(0.4f, 0.4f);

        if (sr != null) sr.color = new Color(1f, 0.2f, 0.1f);
        yield return new WaitForSeconds(0.4f);

        float chargeDir = (playerTransform.position.x > transform.position.x) ? 1f : -1f;
        if (sr != null) sr.flipX = (chargeDir < 0);

        float chargeTimer = 0f;
        float chargeDuration = 1.3f;
        while (chargeTimer < chargeDuration)
        {
            chargeTimer += Time.deltaTime;
            transform.Translate(Vector3.right * chargeDir * 8.5f * Time.deltaTime, Space.World);

            if (chargeTimer % 0.2f < 0.05f)
            {
                CameraFollow2D.Shake(0.1f, 0.1f);
            }
            yield return null;
        }

        if (sr != null && !isDead) sr.color = isRaging ? new Color(1f, 0.3f, 0.3f) : originalColor;
        yield return new WaitForSeconds(0.4f);
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
        if (sr != null) sr.color = new Color(0.2f, 0.8f, 1f);
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
        PlayerPrefs.SetInt("Boss_Air_Defeated", 1);
        PlayerPrefs.SetInt("Purified_Air", 1);
        PlayerPrefs.Save();
        if (col != null) col.enabled = false;

        KiwiAudio.PlayVictory();
        CameraFollow2D.Shake(0.5f, 0.6f);
        StartCoroutine(PurificationSequence());
    }

    private IEnumerator PurificationSequence()
    {
        float timer = 0f;
        float duration = 2.2f;
        Color pureAirColor = new Color(0.5f, 0.9f, 1f, 0.6f);

        if (rewardSeedSprite != null)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject seed = new GameObject("Semilla Aire");
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
            if (sr != null) sr.color = Color.Lerp(originalColor, pureAirColor, t);
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
        titleStyle.normal.textColor = isRaging ? new Color(1f, 0.35f, 0.2f) : new Color(0.95f, 0.75f, 0.35f);
        string bossTitle = isRaging ? "⚠ COLOSO DE LA COMBUSTIÓN - [SOBRECALENTAMIENTO] ⚠" : "EL COLOSO DE LA COMBUSTIÓN INFAME (ESPÍRITU DEL AIRE)";
        GUI.Label(new Rect(barX - 80, barY - 26, barWidth + 160, 24), bossTitle, titleStyle);

        GUI.Box(new Rect(barX - 3, barY - 3, barWidth + 6, barHeight + 6), GUIContent.none);

        float healthPct = Mathf.Clamp01((float)currentHealth / maxHealth);
        Color prevColor = GUI.color;
        GUI.color = isRaging ? Color.Lerp(Color.red, Color.yellow, healthPct) : Color.Lerp(new Color(0.8f, 0.3f, 0.1f), new Color(1f, 0.8f, 0.3f), healthPct);
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
