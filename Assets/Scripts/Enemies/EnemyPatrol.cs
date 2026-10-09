using UnityEngine;

namespace Platformer2D
{
    /// <summary>
    /// Inteligencia básica de patrulla para enemigos:
    /// Se desplaza automáticamente y gira al topar con una pared o al detectar un abismo/borde de plataforma.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyPatrol : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float speed = 3f;
        [SerializeField] private bool startsMovingRight = false;

        [Header("Detección de Bordes y Paredes")]
        [Tooltip("Transform ubicado al frente de los pies del enemigo para detectar el vacío")]
        [SerializeField] private Transform groundCheckAhead;
        [Tooltip("Transform ubicado al frente del enemigo para detectar paredes")]
        [SerializeField] private Transform wallCheckAhead;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float checkDistance = 0.5f;

        [Header("Daño")]
        [SerializeField] private int contactDamage = 1;

        private Rigidbody2D rb;
        private bool movingRight;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            movingRight = startsMovingRight;
        }

        private void FixedUpdate()
        {
            // 1. Mover horizontalmente
            float direction = movingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

            // 2. Comprobar abismo frente a los pies
            bool groundInFront = true;
            if (groundCheckAhead != null)
            {
                groundInFront = Physics2D.Raycast(groundCheckAhead.position, Vector2.down, checkDistance, groundLayer);
            }

            // 3. Comprobar pared al frente
            bool wallInFront = false;
            if (wallCheckAhead != null)
            {
                wallInFront = Physics2D.Raycast(wallCheckAhead.position, movingRight ? Vector2.right : Vector2.left, checkDistance, groundLayer);
            }

            // Si no hay suelo al frente o si choca con pared, dar la vuelta
            if (!groundInFront || wallInFront)
            {
                TurnAround();
            }
        }

        private void TurnAround()
        {
            movingRight = !movingRight;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = movingRight;
            }
            else
            {
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }

            // Invertir posición local de los puntos de comprobación
            if (groundCheckAhead != null)
            {
                Vector3 pos = groundCheckAhead.localPosition;
                pos.x = -pos.x;
                groundCheckAhead.localPosition = pos;
            }

            if (wallCheckAhead != null)
            {
                Vector3 pos = wallCheckAhead.localPosition;
                pos.x = -pos.x;
                wallCheckAhead.localPosition = pos;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Daño por contacto con el jugador
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(contactDamage, transform.position);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            if (groundCheckAhead != null)
            {
                Gizmos.DrawLine(groundCheckAhead.position, groundCheckAhead.position + Vector3.down * checkDistance);
            }
            if (wallCheckAhead != null)
            {
                Vector3 dir = movingRight ? Vector3.right : Vector3.left;
                Gizmos.DrawLine(wallCheckAhead.position, wallCheckAhead.position + dir * checkDistance);
            }
        }
    }
}
