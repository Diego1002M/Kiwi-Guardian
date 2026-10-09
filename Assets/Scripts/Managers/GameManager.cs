using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer2D
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Victory
    }

    /// <summary>
    /// Administrador central del juego (Singleton).
    /// Controla las vidas, monedas, puntuación, checkpoint activo y estados de partida.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Configuración Inicial")]
        [SerializeField] private int startingLives = 3;
        [SerializeField] private int coinsForExtraLife = 100;

        [Header("Punto de Inicio por Defecto")]
        [SerializeField] private Transform initialSpawnPoint;

        // Estado del juego
        public GameState CurrentState { get; private set; } = GameState.Playing;
        public int CurrentLives { get; private set; }
        public int CurrentScore { get; private set; }
        public int CurrentCoins { get; private set; }
        public bool HasKey { get; private set; } = false;

        private Vector3 activeCheckpointPosition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CurrentLives = startingLives;
        }

        private void Start()
        {
            if (initialSpawnPoint != null)
            {
                activeCheckpointPosition = initialSpawnPoint.position;
            }
            else
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    activeCheckpointPosition = player.transform.position;
                }
            }

            // Actualizar interfaz inicial
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateLives(CurrentLives);
                UIManager.Instance.UpdateCoins(CurrentCoins);
                UIManager.Instance.UpdateScore(CurrentScore);
                UIManager.Instance.UpdateKeyStatus(HasKey);
            }
        }

        private void Update()
        {
            // Atajo de teclado para pausar el juego con Escape o P
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (CurrentState == GameState.Playing)
                {
                    PauseGame();
                }
                else if (CurrentState == GameState.Paused)
                {
                    ResumeGame();
                }
            }
        }

        public void AddCoins(int amount)
        {
            CurrentCoins += amount;
            AddScore(amount * 10);

            // Cada X monedas otorga una vida extra
            if (CurrentCoins >= coinsForExtraLife)
            {
                CurrentCoins -= coinsForExtraLife;
                AddLife(1);
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateCoins(CurrentCoins);
            }
        }

        public void AddScore(int points)
        {
            CurrentScore += points;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScore(CurrentScore);
            }
        }

        public void AddLife(int amount)
        {
            CurrentLives += amount;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateLives(CurrentLives);
                UIManager.Instance.ShowNotification("+1 Vida Extra!");
            }
        }

        public void PickUpKey()
        {
            HasKey = true;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateKeyStatus(true);
                UIManager.Instance.ShowNotification("¡Llave encontrada! La puerta está lista.");
            }
        }

        public void SetCheckpoint(Vector3 position)
        {
            activeCheckpointPosition = position;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNotification("¡Punto de control guardado!");
            }
        }

        /// <summary>
        /// Maneja la muerte del jugador (pérdida de vida y respawn o Game Over).
        /// </summary>
        public void OnPlayerDied(PlayerHealth player)
        {
            CurrentLives--;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateLives(CurrentLives);
            }

            if (CurrentLives > 0)
            {
                // Reaparecer en el último checkpoint
                player.Respawn(activeCheckpointPosition);
            }
            else
            {
                // Fin de la partida
                TriggerGameOver();
            }
        }

        public void TriggerGameOver()
        {
            CurrentState = GameState.GameOver;
            Time.timeScale = 0f;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameOverPanel(CurrentScore);
            }
        }

        public void CompleteLevel()
        {
            CurrentState = GameState.Victory;
            Time.timeScale = 0f;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowVictoryPanel(CurrentScore);
            }
        }

        public void PauseGame()
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPausePanel(true);
            }
        }

        public void ResumeGame()
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPausePanel(false);
            }
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}
