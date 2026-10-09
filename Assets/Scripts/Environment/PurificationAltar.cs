using UnityEngine;
using Platformer2D;

/// <summary>
/// Altar de Purificación de la Última Semilla.
/// Al llegar Kiwi, restaura el ecosistema y purifica la región del Mar de Plástico.
/// </summary>
public class PurificationAltar : MonoBehaviour
{
    [Header("Efectos")]
    [SerializeField] private Color purifiedSkyColor = new Color(0.1f, 0.4f, 0.45f);
    [SerializeField] private Sprite purifiedSprite;

    private bool isPurified = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPurified) return;

        if (collision.CompareTag("Player"))
        {
            Purify(collision.gameObject);
        }
    }

    private void Purify(GameObject player)
    {
        isPurified = true;

        // Restaurar salud de Kiwi
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null) health.Heal(health.MaxHealth);

        // Cambiar color de fondo de la cámara para mostrar la purificación del agua
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = purifiedSkyColor;
        }

        // Cambiar sprite visual del altar
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && purifiedSprite != null)
        {
            sr.sprite = purifiedSprite;
            sr.color = Color.white;
        }

        Debug.Log("🌱 ¡ZONA PURIFICADA! El Mar de Plástico comienza a sanar gracias a la Última Semilla de Kiwi.");
    }
}
