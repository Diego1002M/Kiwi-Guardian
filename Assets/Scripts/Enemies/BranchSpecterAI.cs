using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Espectro de Ramas (Enemigo regional del Bosque Marchito).
/// Raíz viva corrompida por residuos químicos.
/// DEBILIDAD CRÍTICA AL FUEGO: Las dagas de fuego [2 / Q] le causan daño doble (2x) y lo queman.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BranchSpecterAI : MonoBehaviour, IDamageable, IElementalTarget
{
    [Header("Estadísticas")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float detectionRange = 7.5f;

    [Header("Recompensa")]
    [SerializeField] private Sprite rewardSeedSprite;

    private int currentHealth;
    private Transform playerTransform;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDead = false;
    private Color originalColor;

    public ElementType TargetElement => ElementType.Tierra;

    public void SetRewardSprite(Sprite s) => rewardSeedSprite = s;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        FindPlayer();
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist <= detectionRange)
        {
            float dirX = Mathf.Sign(playerTransform.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(dirX * walkSpeed, rb.linearVelocity.y);
            if (sr != null) sr.flipX = (dirX < 0);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
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
            Debug.Log($"🔥 ¡FUEGO CRÍTICO contra el Espectro de Ramas! ({finalDmg} de daño)");
        }
        else
        {
            StartCoroutine(HurtFlash());
        }

        currentHealth -= finalDmg;
        KiwiAudio.PlayEnemyHit();

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
        if (sr != null) sr.color = new Color(1f, 0.45f, 0.1f); // Tinte fuego
        yield return new WaitForSeconds(0.18f);
        if (sr != null && !isDead) sr.color = originalColor;
    }

    private IEnumerator HurtFlash()
    {
        if (sr != null) sr.color = Color.white;
        yield return new WaitForSeconds(0.12f);
        if (sr != null && !isDead) sr.color = originalColor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            KiwiController kiwi = collision.gameObject.GetComponent<KiwiController>();
            if (kiwi != null && collision.transform.position.y > transform.position.y + 0.35f)
            {
                kiwi.StompBounce(14f);
                TakeDamage(1, Vector2.up);
                return;
            }

            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }

    private void Die()
    {
        isDead = true;
        if (col != null) col.enabled = false;
        rb.linearVelocity = Vector2.zero;

        KiwiAudio.PlayFlyPop();

        if (rewardSeedSprite != null)
        {
            GameObject seedObj = new GameObject("Semilla Rama");
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
