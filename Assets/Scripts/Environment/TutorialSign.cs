using UnityEngine;

/// <summary>
/// Cartel interactivo de tutorial e historia del mundo.
/// Muestra un indicador "[E] Leer" al acercarse Kiwi y despliega un cuadro de diálogo elegante.
/// </summary>
public class TutorialSign : MonoBehaviour
{
    [Header("Contenido del Cartel")]
    [SerializeField] private string signTitle = "📜 Técnicas del Guardián";
    [TextArea(3, 6)]
    [SerializeField] private string signMessage = "Presiona [W] dos veces para un Sobre-Salto en el aire.\nUsa [Shift] para el Torbellino y [Q] para cambiar de Elemento.";

    private bool isPlayerNearby = false;
    private bool isOpen = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            isOpen = false;
        }
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;
            if (isOpen) KiwiAudio.PlaySeed();
        }

        if (isOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            isOpen = false;
        }
    }

    private void OnGUI()
    {
        if (!isPlayerNearby) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
        if (screenPos.z <= 0) return;

        // 1. Prompt flotante sobre el cartel si no está abierto
        if (!isOpen)
        {
            float pw = 160;
            float ph = 30;
            Rect promptRect = new Rect(screenPos.x - pw / 2f, Screen.height - screenPos.y - ph, pw, ph);

            GUI.color = new Color(0.06f, 0.1f, 0.16f, 0.92f);
            GUI.Box(promptRect, GUIContent.none);
            GUI.color = Color.white;

            GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.fontSize = 12;
            promptStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);
            GUI.Label(promptRect, "💬 Presiona [E] Leer", promptStyle);
        }
        else
        {
            // 2. Ventana de lectura de cartel
            float dw = Mathf.Min(480, Screen.width - 40);
            float dh = 150;
            Rect dialogRect = new Rect((Screen.width - dw) / 2f, Screen.height - dh - 50, dw, dh);

            GUI.color = new Color(0.04f, 0.08f, 0.14f, 0.96f);
            GUI.Box(dialogRect, GUIContent.none);
            GUI.color = Color.white;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.35f, 0.9f, 1f);
            GUI.Label(new Rect(dialogRect.x + 10, dialogRect.y + 12, dw - 20, 24), signTitle, titleStyle);

            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.fontSize = 12;
            bodyStyle.alignment = TextAnchor.UpperLeft;
            bodyStyle.wordWrap = true;
            bodyStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(dialogRect.x + 20, dialogRect.y + 44, dw - 40, 75), signMessage, bodyStyle);

            GUIStyle closeStyle = new GUIStyle(GUI.skin.label);
            closeStyle.fontSize = 11;
            closeStyle.fontStyle = FontStyle.Italic;
            closeStyle.alignment = TextAnchor.MiddleRight;
            closeStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);
            GUI.Label(new Rect(dialogRect.x + 20, dialogRect.y + dh - 26, dw - 40, 20), "Presiona [E] o [Espacio] para cerrar", closeStyle);
        }
    }
}
