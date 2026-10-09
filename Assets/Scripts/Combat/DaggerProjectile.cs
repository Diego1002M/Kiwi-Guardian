using UnityEngine;
using Platformer2D;

/// <summary>
/// Daga arrojadiza elemental de Kiwi. Se desplaza a gran velocidad e impacta de forma continua
/// contra cualquier enemigo u obstáculo elemental (muros de fuego, raíces, slimes, etc.).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class DaggerProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 19f;
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private float lifetime = 1.3f;
    [SerializeField] private float hitRadius = 0.55f;

    private Vector2 direction = Vector2.right;
    private bool hasHit = false;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    public ElementType Element { get; private set; } = ElementType.Agua;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            rb.gravityScale = 0f;
        }
    }

    public void Setup(bool facingRight, ElementType element = ElementType.Agua)
    {
        direction = facingRight ? Vector2.right : Vector2.left;
        Element = element;
        transform.localScale = new Vector3(facingRight ? 1.3f : -1.3f, 1.3f, 1f);

        // Color y resplandor según el elemento activo
        if (sr != null)
        {
            sr.color = ElementalHelpers.GetColor(element);
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (hasHit) return;

        // Desplazamiento
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Detección activa continua (incluye triggers para asegurar impacto en enemigos y barreras)
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.useLayerMask = true;
        filter.layerMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

        Collider2D[] results = new Collider2D[6];
        int count = Physics2D.OverlapCircle(transform.position, hitRadius, filter, results);

        for (int i = 0; i < count; i++)
        {
            Collider2D c = results[i];
            if (c != null && !c.CompareTag("Player") && c.gameObject != gameObject && c.GetComponent<DaggerProjectile>() == null)
            {
                HandleHit(c);
                if (hasHit) break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit || collision.CompareTag("Player") || collision.gameObject == gameObject || collision.GetComponent<DaggerProjectile>() != null) return;
        HandleHit(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit || collision.collider.CompareTag("Player") || collision.gameObject == gameObject) return;
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D col)
    {
        if (hasHit) return;

        // 1. Comprobar si es un objetivo elemental específico (Muros de fuego, raíces, etc.)
        IElementalTarget elemental = col.GetComponent<IElementalTarget>();
        if (elemental == null) elemental = col.GetComponentInParent<IElementalTarget>();

        if (elemental != null)
        {
            hasHit = true;
            int finalDmg = baseDamage;
            if (ElementalHelpers.IsStrongAgainst(Element, elemental.TargetElement))
            {
                finalDmg *= 2; // ¡Daño crítico elemental!
                CombatFeedback.ShowPopup(col.transform.position, "¡CRÍTICO! 2x", new Color(1f, 0.85f, 0.1f));
                CameraFollow2D.Shake(0.2f, 0.2f);
            }
            else
            {
                CombatFeedback.ShowPopup(col.transform.position, $"{finalDmg}", Color.white);
            }
            elemental.ApplyElementalHit(Element, finalDmg, direction);
            KiwiAudio.PlayEnemyHit();
            Destroy(gameObject);
            return;
        }

        // 2. Comprobar si es un enemigo estándar (IDamageable)
        IDamageable damageable = col.GetComponent<IDamageable>();
        if (damageable == null) damageable = col.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            hasHit = true;
            CombatFeedback.ShowPopup(col.transform.position, $"{baseDamage}", Color.white);
            damageable.TakeDamage(baseDamage, direction);
            KiwiAudio.PlayEnemyHit();
            Destroy(gameObject);
            return;
        }

        // Si tiene etiqueta Enemy
        if (col.CompareTag("Enemy"))
        {
            hasHit = true;
            Destroy(col.gameObject);
            KiwiAudio.PlayEnemyHit();
            Destroy(gameObject);
            return;
        }

        // 3. Choque contra suelo o muros
        if (col.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = ElementalHelpers.GetColor(Element);
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
