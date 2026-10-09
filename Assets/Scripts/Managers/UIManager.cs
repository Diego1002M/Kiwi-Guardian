using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer2D
{
    /// <summary>
    /// Administrador de la interfaz de usuario (HUD y Menús).
    /// Controla corazones de salud, vidas, monedas, puntuación y paneles de Pausa/GameOver/Victoria.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD - Salud y Vidas")]
        [Tooltip("Contenedor o imágenes de corazones")]
        [SerializeField] private Image[] heartImages;
        [SerializeField] private Sprite fullHeartSprite;
        [SerializeField] private Sprite emptyHeartSprite;
        [SerializeField] private Text livesText;

        [Header("HUD - Recursos y Puntos")]
        [SerializeField] private Text coinsText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Image keyIcon;
        [SerializeField] private Color keyInactiveColor = new Color(1f, 1f, 1f, 0.2f);
        [SerializeField] private Color keyActiveColor = Color.yellow;

        [Header("Notificaciones en Pantalla")]
        [SerializeField] private Text notificationText;
        [SerializeField] private float notificationDuration = 2.5f;

        [Header("Paneles de Estado")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverScoreText;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Text victoryScoreText;

        private Coroutine notificationCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // Ocultar paneles iniciales
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (notificationText != null) notificationText.gameObject.SetActive(false);
            if (keyIcon != null) keyIcon.color = keyInactiveColor;
        }

        public void UpdateHealth(int currentHealth, int maxHealth)
        {
            if (heartImages == null || heartImages.Length == 0) return;

            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] == null) continue;

                // Si hay más corazones en el array que la vida máxima
                if (i >= maxHealth)
                {
                    heartImages[i].gameObject.SetActive(false);
                    continue;
                }

                heartImages[i].gameObject.SetActive(true);

                if (i < currentHealth)
                {
                    if (fullHeartSprite != null) heartImages[i].sprite = fullHeartSprite;
                    heartImages[i].color = Color.white;
                }
                else
                {
                    if (emptyHeartSprite != null) heartImages[i].sprite = emptyHeartSprite;
                    heartImages[i].color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                }
            }
        }

        public void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                livesText.text = "x " + lives.ToString();
            }
        }

        public void UpdateCoins(int coins)
        {
            if (coinsText != null)
            {
                coinsText.text = coins.ToString("D2");
            }
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = "SCORE: " + score.ToString();
            }
        }

        public void UpdateKeyStatus(bool hasKey)
        {
            if (keyIcon != null)
            {
                keyIcon.color = hasKey ? keyActiveColor : keyInactiveColor;
            }
        }

        public void ShowNotification(string message)
        {
            if (notificationText == null) return;

            if (notificationCoroutine != null)
            {
                StopCoroutine(notificationCoroutine);
            }

            notificationCoroutine = StartCoroutine(NotificationRoutine(message));
        }

        private IEnumerator NotificationRoutine(string message)
        {
            notificationText.text = message;
            notificationText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(notificationDuration);
            notificationText.gameObject.SetActive(false);
        }

        public void ShowPausePanel(bool isPaused)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(isPaused);
            }
        }

        public void ShowGameOverPanel(int finalScore)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            if (gameOverScoreText != null)
            {
                gameOverScoreText.text = "Puntuación Final: " + finalScore.ToString();
            }
        }

        public void ShowVictoryPanel(int finalScore)
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
            if (victoryScoreText != null)
            {
                victoryScoreText.text = "¡Nivel Completado!\nPuntos: " + finalScore.ToString();
            }
        }

        // Métodos vinculables directamente a botones en el Inspector de Unity
        public void OnClickResume()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        public void OnClickRestart()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
            }
        }

        public void OnClickQuit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
