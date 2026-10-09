using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Mosca de Basura mutada (Enemigo volador transversal del Mar de Plástico).
/// Flota oscilando y persigue a Kiwi. Se puede derrotar lanzándole una daga O pisándola desde arriba.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TrashFlyAI : MonoBehaviour, IDamageable
{
    [Header("Estadísticas")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private float flySpeed = 2.8f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float bobbingSpeed = 4f;
    [SerializeField] private float bobbingAmount = 0.5f;

    [Header("Recompensa")]
    [SerializeField] private Sprite rewardSeedSprite;

    private int currentHealth;
    private Transform playerTransform;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private Vector3 initialPos;
    private bool isDead = false;
    private Color originalColor;

    public void SetRewardSprite(Sprite s) => rewardSeedSprite = s;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        currentHealth = maxHealth;
        initialPos = transform.position;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (isDead) return;

        FindPlayer();
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= detectionRange)
        {
            // Perseguir a Kiwi suavemente por el aire
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            transform.position += dir * flySpeed * Time.deltaTime;

            if (sr != null) sr.flipX = (dir.x < 0);
        }
        else
        {
            // Flotar en su puesto aleteando suavemente
            float newY = initialPos.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        // 1. Impacto de Daga de Kiwi
        DaggerProjectile dagger = collision.GetComponent<DaggerProjectile>();
        if (dagger != null)
        {
            TakeDamage(1, Vector2.right);
            Destroy(dagger.gameObject);
            return;
        }

        // 2. Interacción con Kiwi
        if (collision.CompareTag("Player"))
        {
            KiwiController kiwi = collision.GetComponent<KiwiController>();
            // Si Kiwi salta sobre la mosca desde arriba: ¡Pisotón y rebote!
            if (kiwi != null && collision.transform.position.y > transform.position.y + 0.15f)
            {
                kiwi.StompBounce(15f);
                TakeDamage(1, Vector2.up);
                return;
            }

            // Si choca de frente: Daño a Kiwi
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }

    public void TakeDamage(int damageAmount, Vector2 hitDirection)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(FlashHurt());
        }
    }

    private IEnumerator FlashHurt()
    {
        if (sr != null) sr.color = Color.red;
        yield return new WaitForSeconds(0.12f);
        if (sr != null) sr.color = originalColor;
    }

    private void Die()
    {
        isDead = true;
        if (col != null) col.enabled = false;

        KiwiAudio.PlayFlyPop();

        // Soltar Semilla de Energía
        if (rewardSeedSprite != null)
        {
            GameObject seedObj = new GameObject("Semilla de Mosca");
            seedObj.transform.position = transform.position;
            seedObj.transform.localScale = new Vector3(1.4f, 1.4f, 1f);

            SpriteRenderer seedSr = seedObj.AddComponent<SpriteRenderer>();
            seedSr.sprite = rewardSeedSprite;

            CircleCollider2D c = seedObj.AddComponent<CircleCollider2D>();
            c.isTrigger = true;
            c.radius = 0.35f;

            seedObj.AddComponent<EnergySeedPickup>();
        }

        // Efecto visual de desintegración
        StartCoroutine(DeathAnim());
    }

    private IEnumerator DeathAnim()
    {
        float timer = 0f;
        float duration = 0.15f;
        Vector3 origScale = transform.localScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            transform.localScale = Vector3.Lerp(origScale, origScale * 1.5f, t);
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
