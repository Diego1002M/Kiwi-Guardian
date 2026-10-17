using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Robot Torrecilla de Chatarra (Enemigo táctico blindado).
/// Tiene un escudo frontal que bloquea todas las dagas directas.
/// Para vencerlo, Kiwi debe flanquearlo con el Torbellino Acuático [Shift] o pisarlo con el Sobre-Salto.
/// </summary>
public class ScrapTurretAI : MonoBehaviour, IDamageable
{
    [Header("Estadísticas")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float shootInterval = 2.5f;
    [SerializeField] private float detectionRange = 9f;

    [Header("Recompensa y Proyectil")]
    [SerializeField] private Sprite rewardSeedSprite;
    [SerializeField] private Sprite projectileSprite;

    private int currentHealth;
    private Transform playerTransform;
    private SpriteRenderer sr;
    private Collider2D col;
    private float nextShootTime = 0f;
    private bool isFacingRight = true;
    private bool isDead = false;
    private Color originalColor;

    public void SetRewardSprite(Sprite s) => rewardSeedSprite = s;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        col = GetComponent<Collider2D>();
        currentHealth = maxHealth;

        if (projectileSprite == null)
            projectileSprite = Resources.Load<Sprite>("Sprites/GearProjectile");
        if (rewardSeedSprite == null)
            rewardSeedSprite = Resources.Load<Sprite>("Sprites/EnergySeed");
    }

    private void Start()
    {
        FindPlayer();
        nextShootTime = Time.time + Random.Range(1f, 2f);
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

        // Girar hacia Kiwi
        float dirX = playerTransform.position.x - transform.position.x;
        isFacingRight = (dirX > 0);
        if (sr != null) sr.flipX = !isFacingRight;

        // Disparo periódico si Kiwi está a tiro
        if (dist <= detectionRange && Time.time >= nextShootTime)
        {
            StartCoroutine(ShootBurst());
            nextShootTime = Time.time + shootInterval;
        }
    }

    private IEnumerator ShootBurst()
    {
        for (int i = 0; i < 2; i++)
        {
            if (playerTransform == null || isDead) break;

            GameObject bolt = new GameObject("PernoChatarra");
            bolt.tag = "Hazard";
            bolt.transform.position = transform.position + (isFacingRight ? Vector3.right : Vector3.left) * 0.7f;
            bolt.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            SpriteRenderer bSr = bolt.AddComponent<SpriteRenderer>();
            bSr.sprite = projectileSprite != null ? projectileSprite : sr.sprite;
            bSr.color = Color.white;

            CircleCollider2D c = bolt.AddComponent<CircleCollider2D>();
            c.isTrigger = true;
            c.radius = 0.3f;

            Rigidbody2D projRb = bolt.AddComponent<Rigidbody2D>();
            projRb.bodyType = RigidbodyType2D.Kinematic;
            projRb.useFullKinematicContacts = true;

            Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
            bolt.AddComponent<BossProjectile>().Setup(dir, 9f);
            KiwiAudio.PlayEnemyHit();

            yield return new WaitForSeconds(0.2f);
        }
    }

    public void TakeDamage(int damageAmount, Vector2 hitDirection)
    {
        if (isDead) return;

        // Comprobar si el impacto viene de frente (ESCUDO FRONTAL ACTIVO)
        bool hitFromFront = (isFacingRight && hitDirection.x > 0) || (!isFacingRight && hitDirection.x < 0);

        if (hitFromFront)
        {
            // ¡El escudo frontal bloquea el ataque!
            KiwiAudio.PlayEnemyHit();
            StartCoroutine(ShieldFlash());
            Debug.Log("🛡️ ¡El escudo blindado del Robot desvió la daga! Flanquéalo por la espalda.");
            return;
        }

        // Daño por la espalda o desde arriba
        currentHealth -= damageAmount;
        KiwiAudio.PlayEnemyHit();
        StartCoroutine(HurtFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator ShieldFlash()
    {
        if (sr != null) sr.color = Color.cyan; // Resplandor del escudo
        yield return new WaitForSeconds(0.1f);
        if (sr != null && !isDead) sr.color = originalColor;
    }

    private IEnumerator HurtFlash()
    {
        if (sr != null) sr.color = Color.red;
        yield return new WaitForSeconds(0.12f);
        if (sr != null && !isDead) sr.color = originalColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        // Daga
        DaggerProjectile dagger = collision.GetComponent<DaggerProjectile>();
        if (dagger != null)
        {
            Vector2 hitDir = (transform.position - dagger.transform.position).normalized;
            TakeDamage(1, hitDir);
            Destroy(dagger.gameObject);
            return;
        }

        // Pisotón desde arriba
        if (collision.CompareTag("Player"))
        {
            KiwiController kiwi = collision.GetComponent<KiwiController>();
            if (kiwi != null && collision.transform.position.y > transform.position.y + 0.35f)
            {
                kiwi.StompBounce(15f);
                currentHealth -= 1; // Pisotón ignora escudo
                if (currentHealth <= 0) Die();
                else StartCoroutine(HurtFlash());
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

        KiwiAudio.PlayFlyPop();

        if (rewardSeedSprite != null)
        {
            GameObject seedObj = new GameObject("Semilla Robot");
            seedObj.transform.position = transform.position + Vector3.up * 0.3f;
            seedObj.transform.localScale = new Vector3(1.4f, 1.4f, 1f);

            SpriteRenderer seedSr = seedObj.AddComponent<SpriteRenderer>();
            seedSr.sprite = rewardSeedSprite;

            CircleCollider2D c = seedObj.AddComponent<CircleCollider2D>();
            c.isTrigger = true;
            c.radius = 0.35f;

            seedObj.AddComponent<EnergySeedPickup>();
        }

        Destroy(gameObject, 0.05f);
    }
}
