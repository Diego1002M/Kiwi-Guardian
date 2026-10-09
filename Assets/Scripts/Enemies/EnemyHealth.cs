using UnityEngine;

namespace Platformer2D
{
    /// <summary>
    /// Gestiona la vida del enemigo, detección de pisotón en la cabeza (Stomp) estilo Mario
    /// y recompensa de puntuación.
    /// </summary>
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Configuración")]
        [SerializeField] private int maxHealth = 1;
        [SerializeField] private int scoreValue = 100;

        [Header("Detección de Pisotón (Cabeza)")]
        [Tooltip("Transform o posición en la parte superior del enemigo")]
        [SerializeField] private Transform headCheck;
        [Tooltip("Radio del área de pisotón")]
        [SerializeField] private float headCheckRadius = 0.35f;

        [Header("Efectos Opcionales")]
        [SerializeField] private GameObject deathEffectPrefab;

        private int currentHealth;
        private bool isDead = false;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDead) return;

            // Verificar si el jugador cayó desde arriba
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerController2D playerController = collision.gameObject.GetComponent<PlayerController2D>();
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

                // Si el jugador está cayendo y contacta cerca de la cabeza
                if (playerRb != null && playerRb.linearVelocity.y <= 0.1f)
                {
                    // Comprobar si el contacto ocurrió en la zona superior
                    float contactY = collision.GetContact(0).point.y;
                    float enemyCenterY = transform.position.y;

                    if (contactY > enemyCenterY + 0.1f)
                    {
                        // El jugador aplastó al enemigo con éxito
                        if (playerController != null)
                        {
                            playerController.StompBounce();
                        }

                        TakeDamage(1);
                    }
                }
            }
        }

        public void TakeDamage(int damageAmount, Vector2 hitDirection)
        {
            TakeDamage(damageAmount);
        }

        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;

            // Otorgar puntos en GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // Desactivar colliders y destruir
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            Destroy(gameObject, 0.05f);
        }

        private void OnDrawGizmosSelected()
        {
            if (headCheck != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
            }
        }
    }
}
