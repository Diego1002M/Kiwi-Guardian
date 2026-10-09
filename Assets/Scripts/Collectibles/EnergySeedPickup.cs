using UnityEngine;

/// <summary>
/// Semilla de energía o fragmento de purificación que Kiwi recoge por el nivel.
/// </summary>
public class EnergySeedPickup : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 3.5f;
    [SerializeField] private float floatDistance = 0.2f;

    private Vector3 startPos;
    private bool collected = false;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Pequeña oscilación flotante
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * floatSpeed) * floatDistance, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected) return;

        if (collision.CompareTag("Player"))
        {
            collected = true;
            SimpleHeartHUD.seedsCollected++;
            KiwiAudio.PlaySeed();
            Destroy(gameObject);
        }
    }
}
