using UnityEngine;

namespace Platformer2D
{
    /// <summary>
    /// Punto de control (Checkpoint) que guarda la posición segura de reaparición del jugador.
    /// Cambia de color o activa un sprite al cruzarse.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Retroalimentación Visual")]
        [SerializeField] private Color inactiveColor = Color.gray;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private SpriteRenderer flagRenderer;

        private bool isActivated = false;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (flagRenderer == null) flagRenderer = GetComponentInChildren<SpriteRenderer>();
            UpdateVisuals();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isActivated) return;

            if (collision.CompareTag("Player"))
            {
                ActivateCheckpoint();
            }
        }

        public void ActivateCheckpoint()
        {
            isActivated = true;
            UpdateVisuals();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCheckpoint(transform.position);
            }
        }

        private void UpdateVisuals()
        {
            if (flagRenderer != null)
            {
                flagRenderer.color = isActivated ? activeColor : inactiveColor;
            }
        }
    }
}
