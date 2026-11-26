using UnityEngine;

namespace Platformer2D
{
    /// <summary>
    /// Cámara de seguimiento 2D cinemática inspirada en Hollow Knight y Celeste:
    /// - Anticipación de movimiento (Lookahead): la cámara se adelanta en la dirección que corre Kiwi.
    /// - Seguimiento suave y receptivo que nunca pierde de vista al personaje.
    /// - Posicionamiento instantáneo al iniciar la escena.
    /// - Efectos de sacudida de pantalla (Screen Shake) dinámicos para impactos épicos.
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Objetivo")]
        [SerializeField] private Transform target;

        [Header("Desplazamiento y Anticipación (Lookahead)")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, -10f);
        [SerializeField] private float smoothTimeX = 0.12f; // Respuesta rápida en horizontal
        [SerializeField] private float smoothTimeY = 0.18f; // Respuesta suave en vertical
        [SerializeField] private float lookaheadDistance = 2.0f; // Distancia de adelanto hacia donde corre Kiwi
        [SerializeField] private float lookaheadSpeed = 3.5f;

        [Header("Límites del Nivel (Boundaries)")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-20f, -6f);
        [SerializeField] private Vector2 maxBounds = new Vector2(100f, 25f);

        private float currentLookaheadX = 0f;
        private Vector3 currentVelocity = Vector3.zero;
        private Rigidbody2D targetRb;

        private void Start()
        {
            FindTarget();
            // Centrado instantáneo inicial para que no tarde en viajar hasta Kiwi al cargar el nivel
            if (target != null)
            {
                transform.position = new Vector3(target.position.x + offset.x, target.position.y + offset.y, offset.z);
            }
        }

        private void FindTarget()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                    targetRb = player.GetComponent<Rigidbody2D>();
                }
            }
            else if (targetRb == null)
            {
                targetRb = target.GetComponent<Rigidbody2D>();
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindTarget();
                if (target == null) return;
            }

            // 1. Anticipación en la dirección de avance (Lookahead como en los grandes juegos 2D)
            float moveDirection = 0f;
            if (targetRb != null)
            {
                float vx = targetRb.linearVelocity.x;
                if (Mathf.Abs(vx) > 0.5f)
                {
                    moveDirection = Mathf.Sign(vx);
                }
            }
            else
            {
                moveDirection = Mathf.Sign(target.localScale.x);
            }

            float targetLookaheadX = moveDirection * lookaheadDistance;
            currentLookaheadX = Mathf.MoveTowards(currentLookaheadX, targetLookaheadX, lookaheadSpeed * Time.deltaTime);

            // 2. Posición deseada
            Vector3 desiredPosition = new Vector3(
                target.position.x + offset.x + currentLookaheadX,
                target.position.y + offset.y,
                offset.z
            );

            // 3. Límites opcionales
            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
            }

            // 4. Suavizado en X e Y
            float newX = Mathf.SmoothDamp(transform.position.x, desiredPosition.x, ref currentVelocity.x, smoothTimeX);
            float newY = Mathf.SmoothDamp(transform.position.y, desiredPosition.y, ref currentVelocity.y, smoothTimeY);

            transform.position = new Vector3(newX, newY, offset.z);

            // 5. Efecto de sacudida de pantalla (Screen Shake)
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
            if (newTarget != null) targetRb = newTarget.GetComponent<Rigidbody2D>();
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
