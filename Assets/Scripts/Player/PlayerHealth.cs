using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer2D
{
    /// <summary>
    /// Gestiona la salud del jugador, vidas, frames de invulnerabilidad (I-Frames)
    /// y pantalla de Game Over con opciones de Reintentar o Comenzar desde Cero.
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
        private bool isDead = false;
        private PlayerController2D controller;
        private KiwiController kiwiController;
        private KiwiCombat kiwiCombat;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            // Asegurar que el tiempo siempre esté activo al entrar o reiniciar el nivel
            Time.timeScale = 1f;
            isDead = false;

            controller = GetComponent<PlayerController2D>();
            kiwiController = GetComponent<KiwiController>();
            kiwiCombat = GetComponent<KiwiCombat>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            currentHealth = maxHealth;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
            }
        }

        private void Update()
        {
            // Atajos de teclado en pantalla de muerte (funcionan incluso con Time.timeScale = 0)
            if (isDead)
            {
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    RetryLevel();
                }
                else if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape))
                {
                    ReturnToMainMenu();
                }
            }
        }

        /// <summary>
        /// Aplica daño al jugador, activa invulnerabilidad y empuje.
        /// </summary>
        public void TakeDamage(int amount, Vector2 damageSourcePosition)
        {
            if (isInvulnerable || isDead) return;

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
                Vector2 knockbackDirection = (transform.position - (Vector3)damageSourcePosition).normalized;
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
            if (isDead) return;
            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// Muerte de Kiwi por pérdida de corazones o caída al vacío.
        /// Despliega la pantalla de Game Over con opciones de Reintentar o Menú Principal.
        /// </summary>
        public void Die()
        {
            if (isDead) return;
            isDead = true;

            // Audio de derrota retro
            KiwiAudio.PlayPlayerDeath();
            CameraFollow2D.Shake(0.5f, 0.5f);

            // Detener movimiento y combate de Kiwi
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }

            if (controller != null) controller.enabled = false;
            if (kiwiController != null) kiwiController.enabled = false;
            if (kiwiCombat != null) kiwiCombat.enabled = false;

            // Tinte sombrío de derrota
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.45f, 0.45f, 0.55f, 0.85f);
            }

            // Pausar el tiempo de juego
            Time.timeScale = 0f;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDied(this);
            }
        }

        /// <summary>
        /// Reinicia el nivel actual de inmediato con salud restaurada.
        /// </summary>
        public void RetryLevel()
        {
            Time.timeScale = 1f;
            isDead = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Regresa al Menú Principal para comenzar el juego desde cero.
        /// </summary>
        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            isDead = false;
            SimpleHeartHUD.seedsCollected = 0;
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Restablece la salud y reubica al jugador en la posición del checkpoint.
        /// </summary>
        public void Respawn(Vector3 respawnPosition)
        {
            transform.position = respawnPosition;
            currentHealth = maxHealth;
            isInvulnerable = false;
            isDead = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 3f;
            }

            if (controller != null) controller.enabled = true;
            if (kiwiController != null) kiwiController.enabled = true;
            if (kiwiCombat != null) kiwiCombat.enabled = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = Color.white;
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

        private void OnGUI()
        {
            if (!isDead) return;

            // 1. Telón oscuro de fondo con viñeta
            float sw = Screen.width;
            float sh = Screen.height;

            Color prevGuiColor = GUI.color;
            GUI.color = new Color(0.04f, 0.06f, 0.1f, 0.88f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = prevGuiColor;

            // 2. Ventana Modal Centrada de Game Over
            float winW = Mathf.Min(480f, sw * 0.9f);
            float winH = 320f;
            float winX = (sw - winW) / 2f;
            float winY = (sh - winH) / 2f;

            // Marco de la ventana
            GUI.Box(new Rect(winX, winY, winW, winH), GUIContent.none);

            // Título Principal
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontSize = 24;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
            GUI.Label(new Rect(winX, winY + 20, winW, 36), "💀 ¡KIWI HA CAÍDO! 💀", titleStyle);

            // Subtítulo de historia
            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.alignment = TextAnchor.MiddleCenter;
            subStyle.fontSize = 13;
            subStyle.fontStyle = FontStyle.Italic;
            subStyle.normal.textColor = new Color(0.85f, 0.85f, 0.9f);
            GUI.Label(new Rect(winX + 20, winY + 62, winW - 40, 24), "La última semilla peligra ante la corrupción de las sombras...", subStyle);

            // Caja de información del nivel
            GUI.Box(new Rect(winX + 30, winY + 95, winW - 60, 48), GUIContent.none);

            GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
            infoStyle.alignment = TextAnchor.MiddleCenter;
            infoStyle.fontSize = 13;
            infoStyle.normal.textColor = new Color(0.4f, 0.9f, 1f);
            string regionName = SceneManager.GetActiveScene().name;
            GUI.Label(new Rect(winX + 30, winY + 98, winW - 60, 20), $"📍 Región: {regionName}", infoStyle);

            infoStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
            GUI.Label(new Rect(winX + 30, winY + 118, winW - 60, 20), $"🌱 Semillas de Energía: {SimpleHeartHUD.seedsCollected}", infoStyle);

            // 3. Botón 1: REINTENTAR NIVEL
            float btnW = winW - 70;
            float btnH = 44;
            float btnX = winX + 35;

            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.85f, 0.45f);
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 14;
            btnStyle.fontStyle = FontStyle.Bold;
            btnStyle.normal.textColor = Color.white;

            if (GUI.Button(new Rect(btnX, winY + 160, btnW, btnH), "🔄  REINTENTAR NIVEL  [R / Espacio]", btnStyle))
            {
                RetryLevel();
            }

            // 4. Botón 2: COMENZAR DESDE CERO / MENÚ PRINCIPAL
            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.25f);
            if (GUI.Button(new Rect(btnX, winY + 215, btnW, btnH), "🏠  COMENZAR DESDE CERO  [M / Esc]", btnStyle))
            {
                ReturnToMainMenu();
            }
            GUI.backgroundColor = prevColor;

            // 5. Atajo en texto pequeño
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
            hintStyle.alignment = TextAnchor.MiddleCenter;
            hintStyle.fontSize = 11;
            hintStyle.normal.textColor = new Color(0.6f, 0.65f, 0.75f);
            GUI.Label(new Rect(winX, winY + 274, winW, 20), "Presiona [R] para volver a intentarlo o [M] para reiniciar desde el Menú", hintStyle);
        }
    }
}
