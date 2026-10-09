using UnityEngine;

namespace Platformer2D
{
    /// <summary>
    /// Zona de vacío / abismo colocada debajo del nivel.
    /// Al tocarla, el jugador pierde una vida y reaparece en el último checkpoint.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class KillZone : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Die();
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                Gizmos.DrawCube(transform.position + (Vector3)col.offset, col.size);
            }
        }
    }
}
