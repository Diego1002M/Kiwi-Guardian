using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Slime Tóxico de Fuego (Enemigo regional del Desierto de Residuos).
/// Salta hacia Kiwi y quema por contacto.
/// DEBILIDAD CRÍTICA AL AGUA: Las dagas de agua le hacen daño doble (2x) y apagan sus llamas.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FireSlimeAI : MonoBehaviour, IDamageable, IElementalTarget
{
    [Header("Estadísticas")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float jumpInterval = 2.4f;
    [SerializeField] private float jumpForceX = 3.2f;
    [SerializeField] private float jumpForceY = 6.5f;
    [SerializeField] private float detectionRange = 8f;

    [Header("Recompensa")]
    [SerializeField] private Sprite rewardSeedSprite;

    private int currentHealth;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private Transform playerTransform;

    private Color flameColor = new Color(1f, 0.45f, 0.1f);
    private Color cooledColor = new Color(0.4f, 0.4f, 0.45f); // Apagado por agua
    private bool isExtinguished = false;
    private bool isDead = false;
    private float nextJumpTime = 0f;

    public ElementType TargetElement => ElementType.Fuego;

    public void SetRewardSprite(Sprite s) => rewardSeedSprite = s;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        currentHealth = maxHealth;

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        FindPlayer();
        nextJumpTime = Time.time + Random.Range(1f, 2f);
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

        // Brillo de llamas pulsante si no está apagado
        if (sr != null && !isExtinguished)
        {
            float pulse = 0.85f + Mathf.PingPong(Time.time * 5f, 0.3f);
            sr.color = flameColor * pulse;
        }

        // Saltos periódicos hacia Kiwi
        if (dist <= detectionRange && Time.time >= nextJumpTime)
        {
            float dirX = Mathf.Sign(playerTransform.position.x - transform.position.x);
            float jumpX = isExtinguished ? jumpForceX * 0.5f : jumpForceX;
            float jumpY = isExtinguished ? jumpForceY * 0.7f : jumpForceY;

            rb.linearVelocity = new Vector2(dirX * jumpX, jumpY);
            nextJumpTime = Time.time + jumpInterval;
        }
    }

    public void ApplyElementalHit(ElementType element, int damage, Vector2 hitDirection)
    {
        if (isDead) return;

        // 🌊 ¡DEBILIDAD AL AGUA!
        if (element == ElementType.Agua)
        {
            int critDamage = damage * 2; // Daño doble
            currentHealth -= critDamage;
            StartCoroutine(ExtinguishRoutine());
            KiwiAudio.PlayFlyPop(); // Sonido de vapor
            Debug.Log($"🌊 ¡GOLPE CRÍTICO DE AGUA al Slime de Fuego! ({critDamage} de daño)");
        }
        else
        {
            // Otros elementos hacen daño normal
            currentHealth -= damage;
            StartCoroutine(HurtFlash());
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

    private IEnumerator ExtinguishRoutine()
    {
        isExtinguished = true;
        if (sr != null) sr.color = cooledColor;

        yield return new WaitForSeconds(3.0f);

        // Volver a encenderse si sigue vivo
        if (!isDead)
        {
            isExtinguished = false;
            if (sr != null) sr.color = flameColor;
        }
    }

    private IEnumerator HurtFlash()
    {
        if (sr != null) sr.color = Color.white;
        yield return new WaitForSeconds(0.12f);
        if (sr != null && !isDead) sr.color = isExtinguished ? cooledColor : flameColor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            KiwiController kiwi = collision.gameObject.GetComponent<KiwiController>();
            // Pisotón si Kiwi cae desde arriba
            if (kiwi != null && collision.transform.position.y > transform.position.y + 0.35f)
            {
                kiwi.StompBounce(14f);
                TakeDamage(1, Vector2.up);
                return;
            }

            // Daño a Kiwi por contacto
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
            GameObject seedObj = new GameObject("Semilla de Slime");
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
