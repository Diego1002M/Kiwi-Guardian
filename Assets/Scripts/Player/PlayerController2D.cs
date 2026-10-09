using UnityEngine;

namespace Platformer2D
{
    /// <summary>
    /// Controlador principal del jugador: gestiona movimiento horizontal,
    /// salto variable con gravedad dinámica, coyote time, jump buffering y pisotón (stomp).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movimiento")]
        [Tooltip("Velocidad máxima horizontal")]
        [SerializeField] private float moveSpeed = 8f;
        [Tooltip("Suavizado de aceleración/desaceleración")]
        [Range(0f, 0.3f)]
        [SerializeField] private float movementSmoothing = 0.05f;

        [Header("Salto")]
        [Tooltip("Fuerza del salto")]
        [SerializeField] private float jumpForce = 14f;
        [Tooltip("Multiplicador de gravedad al caer para un salto más rápido y satisfactorio")]
        [SerializeField] private float fallMultiplier = 2.5f;
        [Tooltip("Multiplicador de gravedad si se suelta el botón de salto prematuramente")]
        [SerializeField] private float lowJumpMultiplier = 2f;
        [Tooltip("Fuerza de rebote al pisar la cabeza de un enemigo")]
        [SerializeField] private float stompBounceForce = 12f;

        [Header("Detección de Suelo")]
        [Tooltip("Transform ubicado en los pies del jugador")]
        [SerializeField] private Transform groundCheck;
        [Tooltip("Radio del círculo de detección de suelo")]
        [SerializeField] private float groundCheckRadius = 0.2f;
        [Tooltip("Capa que representa el suelo")]
        [SerializeField] private LayerMask groundLayer;

        [Header("Asistencia de Salto (Game Feel)")]
        [Tooltip("Tiempo en segundos tras caer de una plataforma durante el cual aún se puede saltar")]
        [SerializeField] private float coyoteTime = 0.15f;
        [Tooltip("Tiempo en segundos que se guarda la pulsación de salto antes de tocar el suelo")]
        [SerializeField] private float jumpBufferTime = 0.15f;

        [Header("Componentes Visuales")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        // Componentes internos
        private Rigidbody2D rb;
        private Vector3 currentVelocity = Vector3.zero;

        // Variables de estado
        private float horizontalInput;
        private bool isGrounded;
        private bool facingRight = true;
        private float coyoteCounter;
        private float jumpBufferCounter;
        private bool isKnockedBack = false;

        public bool IsGrounded => isGrounded;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            // Bloquea input temporal si el jugador está recibiendo retroceso (Knockback)
            if (isKnockedBack) return;

            // 1. Lectura de Input horizontal (A/D o Flechas)
            horizontalInput = Input.GetAxisRaw("Horizontal");

            // 2. Temporizadores de Coyote Time
            if (isGrounded)
            {
                coyoteCounter = coyoteTime;
            }
            else
            {
                coyoteCounter -= Time.deltaTime;
            }

            // 3. Temporizador de Jump Buffering
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            // 4. Ejecutar Salto si se cumplen las condiciones de coyote y buffer
            if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            {
                ExecuteJump();
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
            }

            // 5. Animación de correr
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
            }

            // 6. Volteo de orientación (Flip)
            if (horizontalInput > 0 && !facingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && facingRight)
            {
                Flip();
            }
        }

        private void FixedUpdate()
        {
            // Detección física de suelo
            CheckGrounded();

            if (!isKnockedBack)
            {
                // Movimiento horizontal suave
                Vector2 targetVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
                rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, movementSmoothing);
            }

            // Modificador de gravedad dinámico (Mario-style Jump Physics)
            ApplyDynamicGravity();
        }

        private void CheckGrounded()
        {
            if (groundCheck == null)
            {
                isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);
                return;
            }

            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        private void ExecuteJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        private void ApplyDynamicGravity()
        {
            // Caída más rápida que la subida
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime);
            }
            // Si soltamos el botón de salto en plena subida, corta el salto
            else if (rb.linearVelocity.y > 0 && !(Input.GetButton("Jump") || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)))
            {
                rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Aplica un rebote cuando el jugador salta sobre un enemigo desde arriba.
        /// </summary>
        public void StompBounce()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounceForce);
            coyoteCounter = 0f;
        }

        /// <summary>
        /// Aplica una fuerza de empuje (Knockback) al recibir daño de una trampa o enemigo.
        /// </summary>
        public void ApplyKnockback(Vector2 direction, float force, float duration = 0.2f)
        {
            if (isKnockedBack) return;
            StartCoroutine(KnockbackRoutine(direction, force, duration));
        }

        private System.Collections.IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
        {
            isKnockedBack = true;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
            yield return new WaitForSeconds(duration);
            isKnockedBack = false;
        }

        private void Flip()
        {
            facingRight = !facingRight;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !facingRight;
            }
            else
            {
                Vector3 scaler = transform.localScale;
                scaler.x *= -1;
                transform.localScale = scaler;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}
