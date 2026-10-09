using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Jefe Regional del Desierto de Residuos: La Colmena Tóxica (Espíritu de Fuego Corrompido).
/// Caldera mecánica viviente que escupe fuego, incendia el suelo e invoca mini-slimes.
/// DEBILIDAD CRÍTICA AL AGUA: Las dagas de agua le hacen daño doble (2x).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ToxicHiveBossAI : MonoBehaviour, IDamageable, IElementalTarget
{
    [Header("Estadísticas")]
    [SerializeField] private int maxHealth = 28;
    [SerializeField] private float moveSpeed = 2.4f;
    [SerializeField] private float arenaMinX = 35f;
    [SerializeField] private float arenaMaxX = 47f;

    [Header("Ataques")]
    [SerializeField] private float attackInterval = 3.2f;
    [SerializeField] private Sprite fireballSprite;
    [SerializeField] private Sprite rewardSeedSprite;

    private int currentHealth;
    private bool isRaging = false;
    private bool isDead = false;
    private float nextAttackTime = 0f;
    private int moveDir = 1;

    private Transform playerTransform;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private Color originalColor;

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

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.gravityScale = 0f;

        currentHealth = maxHealth;
        ActiveBoss = this;
    }

    private void Start()
    {
        FindPlayer();
        nextAttackTime = Time.time + 2f;
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

        // 1. Patrulla flotante/balanceante de la caldera
        transform.Translate(Vector3.right * moveDir * moveSpeed * Time.deltaTime, Space.World);

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

        // 2. Patrones de Ataque
        if (Time.time >= nextAttackTime)
        {
            ExecuteRandomAttack();
            nextAttackTime = Time.time + (isRaging ? attackInterval * 0.6f : attackInterval);
        }
    }

    private void ExecuteRandomAttack()
    {
        int atk = Random.Range(0, 3);
        if (atk == 0)
        {
            // Ataque 1: Morteros de Fuego en arco
            StartCoroutine(MortarAttack());
        }
        else if (atk == 1)
        {
            // Ataque 2: Disparo de fuego al suelo (obliga a saltar o apagar)
            ShootGroundFlame();
        }
        else
        {
            // Ataque 3: Estampida sísmica
            StartCoroutine(SlamAttack());
        }
    }

    private IEnumerator MortarAttack()
    {
        KiwiAudio.PlayBossRoar();
        CameraFollow2D.Shake(0.25f, 0.25f);

        for (int i = 0; i < (isRaging ? 3 : 2); i++)
        {
            if (playerTransform == null) break;

            GameObject fireball = new GameObject("MorteroFuego");
            fireball.tag = "Hazard";
            fireball.transform.position = transform.position + Vector3.up * 1.2f;
            fireball.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            SpriteRenderer fSr = fireball.AddComponent<SpriteRenderer>();
            fSr.sprite = fireballSprite != null ? fireballSprite : sr.sprite;
            fSr.color = new Color(1f, 0.4f, 0.1f);

            CircleCollider2D c = fireball.AddComponent<CircleCollider2D>();
            c.isTrigger = true;
            c.radius = 0.4f;

            Rigidbody2D projRb = fireball.AddComponent<Rigidbody2D>();
            projRb.bodyType = RigidbodyType2D.Kinematic;
            projRb.useFullKinematicContacts = true;

            Vector2 targetPos = playerTransform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0, 0);
            Vector2 launchDir = (targetPos - (Vector2)fireball.transform.position).normalized;

            fireball.AddComponent<BossProjectile>().Setup(launchDir, 8.5f);
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void ShootGroundFlame()
    {
        // Dispara fuego que se clava en el suelo
        KiwiAudio.PlayEnemyHit();
        Vector3 dropPos = new Vector3(playerTransform.position.x, transform.position.y - 1.5f, 0f);

        GameObject fire = new GameObject("LlamaSuelo");
        fire.transform.position = dropPos;
        fire.transform.localScale = new Vector3(1.2f, 1.8f, 1f);

        SpriteRenderer fSr = fire.AddComponent<SpriteRenderer>();
        fSr.sprite = fireballSprite != null ? fireballSprite : sr.sprite;
        fSr.color = new Color(1f, 0.35f, 0.1f);

        BoxCollider2D bc = fire.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(1f, 1.5f);

        // Se apaga al lanzarle agua
        fire.AddComponent<FireBarrier>();
        Destroy(fire, 6f); // Se disuelve a los 6 segundos si no se apaga antes
    }

    private IEnumerator SlamAttack()
    {
        // Estampida hacia abajo y temblor
        Vector3 startPos = transform.position;
        transform.position += Vector3.up * 1.5f;
        yield return new WaitForSeconds(0.3f);

        transform.position = startPos;
        CameraFollow2D.Shake(0.45f, 0.35f);
        KiwiAudio.PlayBossRoar();
    }

    public void ApplyElementalHit(ElementType element, int damage, Vector2 hitDirection)
    {
        if (isDead) return;

        int finalDamage = damage;
        // 🌊 ¡DEBILIDAD AL AGUA!
        if (element == ElementType.Agua)
        {
            finalDamage *= 2;
            KiwiAudio.PlayFlyPop(); // Vapor
            Debug.Log($"🌊 ¡AGUA CRÍTICA contra La Colmena Tóxica! ({finalDamage} de daño)");
        }

        currentHealth -= finalDamage;
        KiwiAudio.PlayEnemyHit();

        if (currentHealth <= maxHealth / 2 && !isRaging)
        {
            isRaging = true;
            KiwiAudio.PlayBossRoar();
            CameraFollow2D.Shake(0.4f, 0.4f);
            if (sr != null) sr.color = new Color(1f, 0.2f, 0.2f); // Rojo incandescente
        }

        StartCoroutine(HurtFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(int damageAmount, Vector2 hitDirection)
    {
        ApplyElementalHit(ElementType.Agua, damageAmount, hitDirection);
    }

    private IEnumerator HurtFlash()
    {
        if (sr != null) sr.color = Color.white;
        yield return new WaitForSeconds(0.12f);
        if (sr != null && !isDead) sr.color = isRaging ? new Color(1f, 0.2f, 0.2f) : originalColor;
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

    private void Die()
    {
        isDead = true;
        if (col != null) col.enabled = false;

        KiwiAudio.PlayVictory();
        CameraFollow2D.Shake(0.5f, 0.6f);

        // Secuencia de purificación del fuego
        StartCoroutine(PurifySequence());
    }

    private IEnumerator PurifySequence()
    {
        float timer = 0f;
        float duration = 2.2f;
        Color pureFlameColor = new Color(1f, 0.85f, 0.3f, 0.6f); // Fuego solar purificado

        if (rewardSeedSprite != null)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject seed = new GameObject("Semilla Fuego");
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
            if (sr != null) sr.color = Color.Lerp(originalColor, pureFlameColor, t);
            transform.position += Vector3.up * 1.2f * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
