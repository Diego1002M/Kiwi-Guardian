using UnityEngine;

namespace Platformer2D
{
    /// <summary>
    /// Cámara de seguimiento 2D suave con límites configurables y offset dinámico.
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Objetivo")]
        [SerializeField] private Transform target;

        [Header("Suavizado y Desplazamiento")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);
        [SerializeField] private float smoothTime = 0.25f;

        [Header("Límites del Nivel (Boundaries)")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(100f, 20f);

        private Vector3 currentVelocity = Vector3.zero;

        private void Start()
        {
            // Si no se asignó manualmente el target, buscar automáticamente al jugador
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;

            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }

            // Mantener siempre el Z de la cámara
            targetPosition.z = offset.z;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

            // Efecto de sacudida de pantalla (Screen Shake)
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                Vector3 shakeOffset = (Vector3)Random.insideUnitCircle * shakeMagnitude;
                transform.position += shakeOffset;
            }
        }

        private static float shakeTimer = 0f;
        private static float shakeMagnitude = 0f;

        public static void Shake(float magnitude = 0.28f, float duration = 0.2f)
        {
            shakeMagnitude = magnitude;
            shakeTimer = duration;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void OnDrawGizmosSelected()
        {
            if (useBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0f);
                Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0f);
                Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0f);
                Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0f);

                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomRight, topRight);
                Gizmos.DrawLine(topRight, topLeft);
                Gizmos.DrawLine(topLeft, bottomLeft);
            }
        }
    }
}
