using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Enemigo regional de la Ciudad de Humo: Nube Tóxica.
/// Espectro de humo contaminante que flota en el aire y libera ráfagas de gas.
/// Se puede derrotar con dagas o pisándolo desde arriba con el Sobre-Salto.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ToxicCloudAI : MonoBehaviour, IDamageable
{
    [Header("Estadísticas")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float floatSpeed = 2.2f;
    [SerializeField] private float detectionRange = 9f;
    [SerializeField] private float puffInterval = 3.5f;

    [Header("Recompensa")]
    [SerializeField] private Sprite rewardSeedSprite;

    private int currentHealth;
    private Transform playerTransform;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDead = false;
    private float nextPuffTime = 0f;
    private Color originalColor;
    private Vector3 initialPos;

    public void SetRewardSprite(Sprite s) => rewardSeedSprite = s;

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
        initialPos = transform.position;
    }

    private void Start()
    {
        FindPlayer();
        nextPuffTime = Time.time + puffInterval;
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

        if (dist <= detectionRange)
        {
            // Flotar lentamente hacia Kiwi
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            transform.position += dir * floatSpeed * Time.deltaTime;

            if (sr != null) sr.flipX = (dir.x < 0);

            // Soltar bocanada de gas periódica
            if (Time.time >= nextPuffTime)
            {
                ReleaseGasPuff();
                nextPuffTime = Time.time + puffInterval;
            }
        }
        else
        {
            // Oscilación suave en el puesto
            float newY = initialPos.y + Mathf.Sin(Time.time * 2.5f) * 0.6f;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void ReleaseGasPuff()
    {
        // Expansión momentánea de gas
        StartCoroutine(GasPuffRoutine());
    }

    private IEnumerator GasPuffRoutine()
    {
        if (sr != null) sr.color = new Color(0.85f, 0.4f, 0.9f); // Tinte gas venenoso
        Vector3 origScale = transform.localScale;
        transform.localScale = origScale * 1.35f;

        yield return new WaitForSeconds(0.4f);

        transform.localScale = origScale;
        if (sr != null) sr.color = originalColor;
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

        // Choque o pisotón de Kiwi
        if (collision.CompareTag("Player"))
        {
            KiwiController kiwi = collision.GetComponent<KiwiController>();
            // Si Kiwi pisa desde arriba usando el Sobre-Salto
            if (kiwi != null && collision.transform.position.y > transform.position.y + 0.3f)
            {
                kiwi.StompBounce(15f);
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
        if (sr != null) sr.color = Color.white;
        yield return new WaitForSeconds(0.12f);
        if (sr != null) sr.color = originalColor;
    }

    private void Die()
    {
        isDead = true;
        if (col != null) col.enabled = false;

        KiwiAudio.PlayFlyPop();

        if (rewardSeedSprite != null)
        {
            GameObject seedObj = new GameObject("Semilla de Nube");
            seedObj.transform.position = transform.position;
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
