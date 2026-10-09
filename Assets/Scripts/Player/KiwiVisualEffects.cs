using System.Collections;
using UnityEngine;

/// <summary>
/// Manejador de "Juice" y Efectos Visuales de Kiwi:
/// - Squash and Stretch dinámico al saltar y aterrizar.
/// - Inclinación del cuerpo al correr (Run Tilt).
/// - Estela de siluetas fantasma de colores (Afterimages) durante el Torbellino Dash.
/// - Anillo de choque de energía durante el Sobre-Salto.
/// - Destellos y partículas de polvo al pisar y saltar.
/// </summary>
public class KiwiVisualEffects : MonoBehaviour
    {
        private KiwiController controller;
        private KiwiCombat combat;
        private SpriteRenderer sr;
        private Transform visualTransform;

        private Vector3 baseScale;
        private Vector3 targetScale;
        private float targetZRotation = 0f;
        private bool wasGroundedLastFrame = true;

        private float nextGhostTime = 0f;
        private const float ghostInterval = 0.045f;

        private void Awake()
        {
            controller = GetComponent<KiwiController>();
            combat = GetComponent<KiwiCombat>();
            sr = GetComponent<SpriteRenderer>();
            visualTransform = transform;
            baseScale = visualTransform.localScale;
            targetScale = baseScale;
        }

        private void Start()
        {
            baseScale = visualTransform.localScale;
            targetScale = baseScale;
        }

        private void Update()
        {
            if (controller == null) return;

            bool isGrounded = controller.IsGrounded;

            // 1. Detección de aterrizaje para Squash
            if (isGrounded && !wasGroundedLastFrame)
            {
                // ¡Aterrizaje! Comprimir horizontalmente
                ApplySquash(1.28f, 0.72f);
                SpawnGroundPuff(0.5f);
            }
            wasGroundedLastFrame = isGrounded;

            // 2. Inclinación al correr (Run Tilt)
            float moveInput = Input.GetAxisRaw("Horizontal");
            if (isGrounded && Mathf.Abs(moveInput) > 0.1f)
            {
                targetZRotation = -Mathf.Sign(moveInput) * 6.5f;
            }
            else
            {
                targetZRotation = 0f;
            }

            // 3. Estela de Dash (Ghost Afterimages)
            if (controller.IsDashing)
            {
                if (Time.time >= nextGhostTime)
                {
                    SpawnGhostAfterimage();
                    nextGhostTime = Time.time + ghostInterval;
                }
            }

            // 4. Suavizado de Escala y Rotación (Spring damping hacia el estado natural)
            targetScale = Vector3.Lerp(targetScale, baseScale, Time.deltaTime * 10f);
            visualTransform.localScale = Vector3.Lerp(visualTransform.localScale, targetScale, Time.deltaTime * 16f);

            float currentZ = visualTransform.localEulerAngles.z;
            if (currentZ > 180f) currentZ -= 360f;
            float newZ = Mathf.Lerp(currentZ, targetZRotation, Time.deltaTime * 14f);
            visualTransform.localEulerAngles = new Vector3(0f, 0f, newZ);
        }

        /// <summary>
        /// Se llama al saltar desde el suelo
        /// </summary>
        public void OnJump()
        {
            ApplyStretch(0.82f, 1.25f);
            SpawnGroundPuff(0.4f);
        }

        /// <summary>
        /// Se llama al realizar el Sobre-Salto (doble salto) en el aire
        /// </summary>
        public void OnDoubleJump()
        {
            ApplyStretch(0.72f, 1.35f);
            SpawnDoubleJumpRing();
        }

        private void ApplyStretch(float scaleXRatio, float scaleYRatio)
        {
            targetScale = new Vector3(baseScale.x * scaleXRatio, baseScale.y * scaleYRatio, baseScale.z);
        }

        private void ApplySquash(float scaleXRatio, float scaleYRatio)
        {
            targetScale = new Vector3(baseScale.x * scaleXRatio, baseScale.y * scaleYRatio, baseScale.z);
        }

        private void SpawnGhostAfterimage()
        {
            if (sr == null || sr.sprite == null) return;

            GameObject ghost = new GameObject("KiwiDashGhost");
            ghost.transform.position = transform.position;
            ghost.transform.localScale = transform.localScale;
            ghost.transform.rotation = transform.rotation;

            SpriteRenderer gSr = ghost.AddComponent<SpriteRenderer>();
            gSr.sprite = sr.sprite;
            gSr.flipX = sr.flipX;
            gSr.sortingOrder = sr.sortingOrder - 1;

            // Color basado en el elemento activo
            Color elemColor = Color.cyan;
            if (combat != null)
            {
                elemColor = ElementalHelpers.GetColor(combat.CurrentElement);
            }
            elemColor.a = 0.65f;
            gSr.color = elemColor;

            ghost.AddComponent<GhostFader>().Setup(0.22f);
        }

        private void SpawnDoubleJumpRing()
        {
            GameObject ring = new GameObject("SobreSaltoRing");
            ring.transform.position = transform.position + Vector3.down * 0.4f;

            SpriteRenderer rSr = ring.AddComponent<SpriteRenderer>();
            rSr.sprite = sr.sprite; // Usar sprite con tinte circular o textura
            rSr.color = new Color(0.2f, 0.95f, 1f, 0.85f);
            rSr.sortingOrder = sr.sortingOrder - 1;

            ring.AddComponent<ExpandAndFadeRing>().Setup(2.5f, 0.3f);
        }

        private void SpawnGroundPuff(float intensity)
        {
            // Pequeño puff de polvo procedural
            GameObject puff = new GameObject("GroundPuff");
            puff.transform.position = transform.position + Vector3.down * 0.5f;

            SpriteRenderer pSr = puff.AddComponent<SpriteRenderer>();
            pSr.sprite = sr.sprite;
            pSr.color = new Color(1f, 1f, 1f, 0.5f);
            puff.transform.localScale = new Vector3(0.6f * intensity, 0.3f * intensity, 1f);

            puff.AddComponent<GhostFader>().Setup(0.18f);
        }
    }

    /// <summary>
    /// Componente que desvanece y destruye una silueta fantasma rápidamente.
    /// </summary>
    public class GhostFader : MonoBehaviour
    {
        private SpriteRenderer sr;
        private float duration = 0.25f;
        private float timer = 0f;
        private Color initialColor;

        public void Setup(float fadeDuration)
        {
            duration = fadeDuration;
        }

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr != null) initialColor = sr.color;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (sr != null)
            {
                sr.color = Color.Lerp(initialColor, new Color(initialColor.r, initialColor.g, initialColor.b, 0f), t);
            }

            transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 1.08f, Time.deltaTime * 4f);

            if (timer >= duration)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Componente para expandir y desvanecer ondas de choque.
    /// </summary>
    public class ExpandAndFadeRing : MonoBehaviour
    {
        private SpriteRenderer sr;
        private float maxScale = 2.2f;
        private float duration = 0.28f;
        private float timer = 0f;
        private Color startColor;

        public void Setup(float scale, float dur)
        {
            maxScale = scale;
            duration = dur;
        }

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr != null) startColor = sr.color;
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(maxScale, maxScale * 0.4f, 1f), t);

            if (sr != null)
            {
                sr.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), t);
            }

            if (timer >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
