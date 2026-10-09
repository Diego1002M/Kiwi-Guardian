using System.Collections;
using UnityEngine;

namespace Platformer2D
{
    /// <summary>
    /// Gestiona la salud del jugador, vidas, frames de invulnerabilidad (I-Frames)
    /// y comunicación con GameManager para el respawn en checkpoints.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Salud")]
        [SerializeField] private int maxHealth = 4;
        [SerializeField] private float invulnerabilityDuration = 1.5f;
        [SerializeField] private float flashInterval = 0.1f;

        [Header("Knockback al recibir daño")]
        [SerializeField] private float damageKnockbackForce = 7f;

        [Header("Referencias Visuales")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private int currentHealth;
        private bool isInvulnerable = false;
        private PlayerController2D controller;
        private KiwiController kiwiController;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        private void Awake()
        {
            controller = GetComponent<PlayerController2D>();
            kiwiController = GetComponent<KiwiController>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            currentHealth = maxHealth;
            // Notificar a la UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// Aplica daño al jugador, activa invulnerabilidad y empuje.
        /// </summary>
        public void TakeDamage(int amount, Vector2 damageSourcePosition)
        {
            if (isInvulnerable) return;

            currentHealth -= amount;
            if (currentHealth < 0) currentHealth = 0;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Empuje en dirección opuesta a la fuente de daño
                Vector2 knockbackDirection = (transform.position - (Vector3)damageSourcePosition).normalized;
                // Asegurar componente vertical para despegar del suelo
                knockbackDirection.y = 0.5f;
                if (controller != null)
                {
                    controller.ApplyKnockback(knockbackDirection, damageKnockbackForce);
                }
                else if (kiwiController != null)
                {
                    kiwiController.ApplyKnockback(knockbackDirection, damageKnockbackForce);
                }

                StartCoroutine(InvulnerabilityRoutine());
            }
        }


        /// <summary>
        /// Restaura puntos de salud (corazones).
        /// </summary>
        public void Heal(int amount)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// Muerte del jugador por pérdida de corazones o caída al vacío.
        /// </summary>
        public void Die()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDied(this);
            }
            else
            {
                // Respawn básico si no hay GameManager en la escena
                Respawn(transform.position);
            }
        }

        /// <summary>
        /// Restablece la salud y reubica al jugador en la posición del checkpoint.
        /// </summary>
        public void Respawn(Vector3 respawnPosition)
        {
            transform.position = respawnPosition;
            currentHealth = maxHealth;
            isInvulnerable = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
            }
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            float elapsed = 0f;

            while (elapsed < invulnerabilityDuration)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                }
                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
            isInvulnerable = false;
        }
    }
}
