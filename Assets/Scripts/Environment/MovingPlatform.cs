using UnityEngine;

namespace Platformer2D
{
    /// <summary>
    /// Plataforma móvil que oscila entre dos o más waypoints.
    /// Hace al jugador hijo de la plataforma para que se desplace naturalmente con ella.
    /// </summary>
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 2.5f;
        [SerializeField] private float waitTimeAtPoint = 0.5f;

        private int currentWaypointIndex = 0;
        private float waitCounter = 0f;
        private bool isWaiting = false;

        private void Update()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            if (isWaiting)
            {
                waitCounter -= Time.deltaTime;
                if (waitCounter <= 0f)
                {
                    isWaiting = false;
                }
                return;
            }

            Transform target = waypoints[currentWaypointIndex];
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 0.05f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                isWaiting = true;
                waitCounter = waitTimeAtPoint;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Solo emparentar si el jugador está pisando desde arriba
            if (collision.gameObject.CompareTag("Player") && collision.transform.position.y > transform.position.y)
            {
                collision.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(null);
            }
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                    int next = (i + 1) % waypoints.Length;
                    if (waypoints[next] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
                    }
                }
            }
        }
    }
}
