using UnityEngine;

/// <summary>
/// Controlador principal de Kiwi para plataformas 2D (Metroidvania).
/// Salto fijo consistente con W, Espacio o Flecha Arriba (sin salto variable por mantener pulsado)
/// y SOBRE-SALTO (Doble salto en el aire con impulso potenciado).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class KiwiController : MonoBehaviour
{
    [Header("Movimiento Horizontal")]
    [Tooltip("Velocidad máxima al correr")]
    [SerializeField] private float moveSpeed = 8.5f;
    [Tooltip("Aceleración hasta alcanzar la velocidad máxima")]
    [SerializeField] private float acceleration = 65f;
    [Tooltip("Freno al soltar los controles")]
    [SerializeField] private float deceleration = 55f;

    [Header("Salto Fijo y Sobre-Salto (Doble Salto)")]
    [Tooltip("Fuerza del primer salto desde el suelo (calibrado para no salirse de la pantalla)")]
    [SerializeField] private float firstJumpForce = 11.2f;
    [Tooltip("Fuerza del SOBRE-SALTO en el aire (impulso extra controlado)")]
    [SerializeField] private float doubleJumpForce = 13.2f;
    [Tooltip("Gravedad base mientras sube")]
    [SerializeField] private float baseGravity = 2.4f;
    [Tooltip("Multiplicador de gravedad al caer")]
    [SerializeField] private float fallGravityMultiplier = 1.45f;
    [Tooltip("Tiempo de gracia tras dejar una plataforma")]
    [SerializeField] private float coyoteTime = 0.18f;
    [Tooltip("Tiempo de buffer al presionar saltar")]
    [SerializeField] private float jumpBufferTime = 0.16f;

    [Header("Detección de Suelo")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.2f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Habilidades Elementales")]
    [Tooltip("Habilidad del Sobre-Salto activa")]
    public bool hasDoubleJump = true;
    private bool canDoubleJump = false;

    [Header("Habilidad de Agua: Torbellino Acuático (Dash)")]
    [SerializeField] private float dashSpeed = 19f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.8f;
    private bool isDashing = false;
    private float nextDashTime = 0f;

    // Componentes
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D mainCollider;

    // Estado interno
    private float horizontalInput;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isFacingRight = true;
    private bool isKnockedBack = false;

    public bool IsGrounded => isGrounded;
    public bool IsFacingRight => isFacingRight;
    public bool IsDashing => isDashing;
    public float DashCooldownRatio => Mathf.Clamp01((nextDashTime - Time.time) / dashCooldown);

    private KiwiVisualEffects visualEffects;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCollider = GetComponent<Collider2D>();
        visualEffects = GetComponent<KiwiVisualEffects>();
        if (visualEffects == null) visualEffects = gameObject.AddComponent<KiwiVisualEffects>();

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        rb.gravityScale = baseGravity;
    }

    private void Update()
    {
        if (isKnockedBack) return;

        // 1. Entrada de control horizontal (Teclas A/D, Flechas)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Comprobar si Kiwi está tocando el suelo
        CheckGround();

        // 3. Temporizadores de Coyote Time y recarga de Sobre-Salto
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 4. Detección de pulsación de salto (W, Espacio, Flecha Arriba)
        bool jumpPressed = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetButtonDown("Jump");

        if (jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // 5. Ejecutar Salto o Sobre-Salto
        if (jumpBufferCounter > 0f)
        {
            if (coyoteTimeCounter > 0f)
            {
                // Primer Salto fijo desde el suelo
                ExecuteFirstJump();
            }
            else if (hasDoubleJump && canDoubleJump)
            {
                // ¡SOBRE-SALTO EN EL AIRE! Segundo salto con mayor impulso
                ExecuteDoubleJump();
            }
        }

        // 6. Gravedad al caer (Snappy feel sin alterar la altura fija por mantener tecla)
        ApplyFallGravity();

        // 7. Torbellino Acuático (Dash con LeftShift, L o C)
        bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.C);
        if (dashPressed && Time.time >= nextDashTime && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }

        // 8. Orientación del sprite
        FlipSprite();
    }

    private void FixedUpdate()
    {
        if (!isKnockedBack && !isDashing)
        {
            ApplyMovement();
        }
    }

    private void ExecuteFirstJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, firstJumpForce);
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        canDoubleJump = true;

        visualEffects?.OnJump();
        KiwiAudio.PlayJump();
    }

    private void ExecuteDoubleJump()
    {
        // Cancelar velocidad vertical previa para garantizar un impulso limpio hacia arriba
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
        canDoubleJump = false;
        jumpBufferCounter = 0f;

        visualEffects?.OnDoubleJump();
        KiwiAudio.PlayDoubleJump();
        SpawnDoubleJumpEffect();
    }

    private void SpawnDoubleJumpEffect()
    {
        // Efecto visual de onda de aire/agua bajo los pies de Kiwi
        GameObject puff = new GameObject("SobreSaltoEffect");
        puff.transform.position = transform.position + Vector3.down * 0.4f;
        SpriteRenderer sr = puff.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.3f, 0.9f, 1f, 0.85f);
        sr.sortingOrder = 5;

        // Cargar textura o crear sprite circular temporal
        Texture2D tex = Texture2D.whiteTexture;
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16);
        puff.transform.localScale = new Vector3(0.5f, 0.2f, 1f);

        StartCoroutine(AnimatePuff(puff, sr));
    }

    private System.Collections.IEnumerator AnimatePuff(GameObject puff, SpriteRenderer sr)
    {
        float timer = 0f;
        float duration = 0.22f;
        Vector3 initialScale = puff.transform.localScale;
        Vector3 targetScale = new Vector3(1.8f, 0.35f, 1f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            if (puff != null)
            {
                puff.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = Mathf.Lerp(0.85f, 0f, t);
                    sr.color = c;
                }
            }
            yield return null;
        }

        if (puff != null) Destroy(puff);
    }

    private void ApplyFallGravity()
    {
        // Gravedad ágil al subir y caer con proporciones equilibradas
        if (rb.linearVelocity.y < -0.01f)
        {
            rb.gravityScale = baseGravity * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    private System.Collections.IEnumerator DashRoutine()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        KiwiAudio.PlayDash();

        rb.gravityScale = 0f;
        float dashDir = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = baseGravity;
        isDashing = false;
    }

    private void ApplyMovement()
    {
        float targetSpeed = horizontalInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);
    }

    private void CheckGround()
    {
        if (groundCheckPoint != null)
        {
            int mask = groundLayer.value != 0 ? groundLayer.value : ~LayerMask.GetMask("Player", "Ignore Raycast");
            isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, mask);
            return;
        }

        if (mainCollider != null)
        {
            Bounds b = mainCollider.bounds;
            Vector2 origin = new Vector2(b.center.x, b.min.y + 0.05f);
            Vector2 boxSize = new Vector2(b.size.x * 0.75f, 0.12f);

            int mask = groundLayer.value != 0 ? groundLayer.value : ~LayerMask.GetMask("Player", "Ignore Raycast");
            RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0f, Vector2.down, 0.18f, mask);
            isGrounded = (hit.collider != null && hit.collider.gameObject != gameObject);
        }
        else
        {
            Vector2 checkPos = (Vector2)transform.position + Vector2.down * 0.9f;
            int mask = groundLayer.value != 0 ? groundLayer.value : ~LayerMask.GetMask("Player", "Ignore Raycast");
            isGrounded = Physics2D.OverlapCircle(checkPos, 0.3f, mask);
        }
    }

    private void FlipSprite()
    {
        if (horizontalInput > 0.01f && !isFacingRight)
        {
            isFacingRight = true;
            if (spriteRenderer != null) spriteRenderer.flipX = false;
            else transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalInput < -0.01f && isFacingRight)
        {
            isFacingRight = false;
            if (spriteRenderer != null) spriteRenderer.flipX = true;
            else transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    /// <summary>
    /// Rebote al pisar o saltar sobre un enemigo (reinicia el Sobre-Salto)
    /// </summary>
    public void StompBounce(float force = 14f)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        coyoteTimeCounter = 0f;
        canDoubleJump = true; // Permite volver a hacer sobre-salto tras rebotar
        KiwiAudio.PlayDoubleJump();
    }

    /// <summary>
    /// Empuje al recibir daño
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        if (groundCheckPoint != null)
        {
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
        else if (mainCollider != null)
        {
            Bounds b = mainCollider.bounds;
            Gizmos.DrawWireCube(new Vector3(b.center.x, b.min.y - 0.05f, 0), new Vector3(b.size.x * 0.75f, 0.15f, 0));
        }
    }
}
