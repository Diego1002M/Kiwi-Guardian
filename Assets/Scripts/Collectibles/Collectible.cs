using UnityEngine;

namespace Platformer2D
{
    public enum CollectibleType
    {
        Coin,
        Heart,
        Key,
        Gem
    }

    /// <summary>
    /// Componente para todos los objetos recolectables del juego (Monedas, Corazones, Llaves, Gemas).
    /// Incluye una pequeña animación procedural de levitación/flotación.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        [Header("Tipo de Coleccionable")]
        [SerializeField] private CollectibleType type = CollectibleType.Coin;
        [SerializeField] private int value = 1; // Puntos o cantidad de curación

        [Header("Animación de Levitación")]
        [SerializeField] private bool floatAnimation = true;
        [SerializeField] private float floatSpeed = 3f;
        [SerializeField] private float floatAmplitude = 0.15f;

        [Header("Efectos Opcionales")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private AudioClip pickupSound;

        private Vector3 startPosition;
        private bool isCollected = false;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            startPosition = transform.position;
        }

        private void Update()
        {
            if (floatAnimation)
            {
                float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isCollected) return;

            if (collision.CompareTag("Player"))
            {
                Collect(collision.gameObject);
            }
        }

        private void Collect(GameObject player)
        {
            isCollected = true;

            switch (type)
            {
                case CollectibleType.Coin:
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddCoins(value);
                    }
                    break;

                case CollectibleType.Heart:
                    PlayerHealth health = player.GetComponent<PlayerHealth>();
                    if (health != null)
                    {
                        health.Heal(value);
                    }
                    break;

                case CollectibleType.Key:
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.PickUpKey();
                    }
                    break;

                case CollectibleType.Gem:
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddScore(value * 50);
                    }
                    break;
            }

            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            Destroy(gameObject);
        }
    }
}
