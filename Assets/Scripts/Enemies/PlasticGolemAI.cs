using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Inteligencia Artificial del Golem de Plástico (Enemigo común del Mar de Plástico).
/// Espera en guardia hasta que Kiwi se acerca, avanza hacia él sin caerse jamás
/// de la plataforma y hace daño por contacto.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlasticGolemAI : MonoBehaviour, IDamageable
{
    [Header("Estadísticas")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float walkSpeed = 2.2f;
    [SerializeField] private float detectionRange = 7.5f;
    [SerializeField] private int contactDamage = 1;

    [Header("Recompensa al morir")]
    [SerializeField] private Sprite rewardSeedSprite;

    [Header("Efectos")]
    [SerializeField] private Color hitColor = new Color(1f, 0.35f, 0.35f);

    private int currentHealth;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private Transform playerTransform;
    private Color originalColor;
    private bool isDead = false;
    private bool isHurt = false;

    // Límites de seguridad de la plataforma para no caerse jamás
    private float platformMinX = float.MinValue;
    private float platformMaxX = float.MaxValue;
    private bool hasPlatformBounds = false;

    public void SetRewardSprite(Sprite s)
    {
        rewardSeedSprite = s;
    }

    public void SetPatrolLimits(float min, float max)
    {
        platformMinX = min;
        platformMaxX = max;
        hasPlatformBounds = true;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (sr != null) originalColor = sr.color;
        currentHealth = maxHealth;

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        FindPlayer();
        DetectPlatformBounds();
    }

    private void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    private void DetectPlatformBounds()
    {
        // Detectar la plataforma debajo de sus pies
        int groundMask = ~LayerMask.GetMask("Enemy", "Player", "Ignore Raycast");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 3f, groundMask);

        if (hit.collider != null)
        {
            Bounds b = hit.collider.bounds;
            platformMinX = b.min.x + 0.85f;
            platformMaxX = b.max.x - 0.85f;
            hasPlatformBounds = true;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isHurt) return;

        FindPlayer();
        if (playerTransform == null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (!hasPlatformBounds)
        {
            DetectPlatformBounds();
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Solo se activa si Kiwi está cerca en su rango de combate
        if (distanceToPlayer <= detectionRange)
        {
            float dirX = playerTransform.position.x - transform.position.x;
            float moveDir = Mathf.Sign(dirX);

            // Voltear sprite hacia Kiwi
            if (sr != null) sr.flipX = (moveDir < 0);

            // SEGURIDAD MÁXIMA: Comprobar límites de la plataforma física
            bool canMove = true;
            if (hasPlatformBounds)
            {
                if (moveDir < 0 && transform.position.x <= platformMinX) canMove = false;
                if (moveDir > 0 && transform.position.x >= platformMaxX) canMove = false;
            }

            // También comprobar con raycast hacia el suelo delantero
            if (canMove && IsGroundAhead(moveDir) && !IsWallAhead(moveDir))
            {
                rb.linearVelocity = new Vector2(moveDir * walkSpeed, rb.linearVelocity.y);
            }
            else
            {
                // Borde alcanzado: el Golem se frena en seco y espera a Kiwi sin caerse jamás
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
        else
        {
            // Kiwi está lejos: El Golem espera quieto vigilando su plataforma
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private bool IsGroundAhead(float dirX)
    {
        if (col == null) return true;

        Bounds b = col.bounds;
        Vector2 checkOrigin = new Vector2(b.center.x + (dirX * (b.extents.x + 0.1f)), b.min.y + 0.15f);

        int groundMask = ~LayerMask.GetMask("Enemy", "Player", "Ignore Raycast");
        RaycastHit2D hit = Physics2D.Raycast(checkOrigin, Vector2.down, 0.45f, groundMask);

        return hit.collider != null;
    }

    private bool IsWallAhead(float dirX)
    {
        if (col == null) return false;

        Bounds b = col.bounds;
        Vector2 checkOrigin = new Vector2(b.center.x + (dirX * (b.extents.x + 0.05f)), b.center.y);
        int groundMask = ~LayerMask.GetMask("Enemy", "Player", "Ignore Raycast");
        RaycastHit2D hit = Physics2D.Raycast(checkOrigin, new Vector2(dirX, 0), 0.25f, groundMask);

        return hit.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // Daño por contacto o pisotón de Kiwi
        if (collision.gameObject.CompareTag("Player"))
        {
            KiwiController kiwi = collision.gameObject.GetComponent<KiwiController>();
            // Si Kiwi cae sobre el Golem desde arriba: ¡Pisotón y rebote!
            if (kiwi != null && collision.transform.position.y > transform.position.y + 0.45f)
            {
                kiwi.StompBounce(14.5f);
                TakeDamage(1, Vector2.up);
                return;
            }

            PlayerHealth pHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(contactDamage, transform.position);
            }
        }
    }

    public void TakeDamage(int damageAmount, Vector2 hitDirection)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        StartCoroutine(HurtRoutine(hitDirection));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HurtRoutine(Vector2 hitDirection)
    {
        isHurt = true;
        if (sr != null) sr.color = hitColor;

        // Retroceso suave al recibir una daga
        rb.linearVelocity = new Vector2(hitDirection.x * 2.5f, 2f);

        yield return new WaitForSeconds(0.18f);

        if (sr != null) sr.color = originalColor;
        isHurt = false;
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        if (col != null) col.enabled = false;

        // Recompensa al morir: suelta una Semilla de Energía
        CreateDeathReward();

        Destroy(gameObject, 0.05f);
    }

    private void CreateDeathReward()
    {
        if (rewardSeedSprite == null) return;

        GameObject seedObj = new GameObject("Semilla de Recompensa");
        seedObj.transform.position = transform.position + Vector3.up * 0.3f;
        seedObj.transform.localScale = new Vector3(1.4f, 1.4f, 1f);

        SpriteRenderer seedSr = seedObj.AddComponent<SpriteRenderer>();
        seedSr.sprite = rewardSeedSprite;
        seedSr.color = Color.white;

        CircleCollider2D c = seedObj.AddComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius = 0.35f;

        seedObj.AddComponent<EnergySeedPickup>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
