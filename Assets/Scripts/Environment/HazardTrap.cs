using UnityEngine;

namespace Platformer2D
{
    /// <summary>
    /// Trampa ambiental de pinchos o sierras que inflige daño al jugador por contacto continuo o disparo.
    /// </summary>
    public class HazardTrap : MonoBehaviour
    {
        [Header("Configuración de Daño")]
        [SerializeField] private int damageAmount = 1;
        [SerializeField] private bool instantKill = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandleDamage(collision.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleDamage(collision.gameObject);
        }

        private void HandleDamage(GameObject target)
        {
            if (target.CompareTag("Player"))
            {
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    if (instantKill)
                    {
                        playerHealth.Die();
                    }
                    else
                    {
                        playerHealth.TakeDamage(damageAmount, transform.position);
                    }
                }
            }
        }
    }
}
