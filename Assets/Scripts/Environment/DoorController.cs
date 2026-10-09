using UnityEngine;

namespace Platformer2D
{
    /// <summary>
    /// Puerta de salida o meta que requiere una llave para abrirse y completar el nivel.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DoorController : MonoBehaviour
    {
        [Header("Requisitos")]
        [SerializeField] private bool requiresKey = true;

        [Header("Retroalimentación Visual")]
        [SerializeField] private SpriteRenderer doorRenderer;
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Color lockedColor = Color.red;
        [SerializeField] private Color openColor = Color.green;

        private bool isOpen = false;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (doorRenderer == null) doorRenderer = GetComponentInChildren<SpriteRenderer>();
            UpdateVisuals();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                TryOpenDoor();
            }
        }

        private void TryOpenDoor()
        {
            if (isOpen) return;

            // Verificar si el jugador tiene la llave necesaria
            if (requiresKey)
            {
                if (GameManager.Instance != null && GameManager.Instance.HasKey)
                {
                    Open();
                }
                else
                {
                    // Notificar en la interfaz que falta la llave
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowNotification("¡Necesitas la llave para abrir esta puerta!");
                    }
                }
            }
            else
            {
                Open();
            }
        }

        private void Open()
        {
            isOpen = true;
            UpdateVisuals();

            // Disparar condición de victoria en GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }
        }

        private void UpdateVisuals()
        {
            if (doorRenderer != null)
            {
                if (openSprite != null && lockedSprite != null)
                {
                    doorRenderer.sprite = isOpen ? openSprite : lockedSprite;
                }
                else
                {
                    doorRenderer.color = isOpen ? openColor : lockedColor;
                }
            }
        }
    }
}
