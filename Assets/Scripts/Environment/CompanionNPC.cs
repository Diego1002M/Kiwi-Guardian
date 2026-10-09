using UnityEngine;

/// <summary>
/// Compañero Pingüino del Mar de Plástico (NPC Aliado).
/// Proporciona información de lore y objetivos a Kiwi al presionar la tecla E.
/// </summary>
public class CompanionNPC : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string npcName = "Pingüino (Guía del Mar de Plástico)";
    [TextArea(3, 6)]
    [SerializeField] private string dialogueText = "Kiwi: «Este lugar antes estaba lleno de vida... ¿qué le pasó?»\n\nPingüino: «La contaminación de plástico corrompió este océano y a su Espíritu guardián. ¡Usa tus dagas [J] o písalas para vencer a las Moscas de basura, y presiona [W] dos veces en el aire para el ¡Sobre-Salto! Lleva la Última Semilla de tu mochila hasta el Altar para salvar el mar.»";

    [SerializeField] private float talkDistance = 3.5f;

    public void SetDialogue(string name, string text)
    {
        npcName = name;
        dialogueText = text;
    }

    private Transform playerTransform;
    private bool isPlayerNear = false;
    private bool isDialogueOpen = false;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerNear = (dist <= talkDistance);

        if (!isPlayerNear)
        {
            isDialogueOpen = false;
        }

        // Abrir o cerrar diálogo con la tecla E
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            isDialogueOpen = !isDialogueOpen;
        }
    }

    private void OnGUI()
    {
        // 1. Indicador para hablar
        if (isPlayerNear && !isDialogueOpen)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
            if (screenPos.z > 0)
            {
                Rect promptRect = new Rect(screenPos.x - 70, Screen.height - screenPos.y - 25, 140, 30);
                GUI.color = new Color(0.1f, 0.15f, 0.22f, 0.9f);
                GUI.Box(promptRect, GUIContent.none);
                GUI.color = Color.yellow;
                GUIStyle pStyle = new GUIStyle(GUI.skin.label);
                pStyle.alignment = TextAnchor.MiddleCenter;
                pStyle.fontStyle = FontStyle.Bold;
                pStyle.fontSize = 13;
                GUI.Label(promptRect, "💬 Presiona [E]", pStyle);
                GUI.color = Color.white;
            }
        }

        // 2. Caja de diálogo en pantalla
        if (isDialogueOpen)
        {
            float boxW = Mathf.Min(650, Screen.width - 40);
            float boxH = 150;
            Rect boxRect = new Rect((Screen.width - boxW) / 2f, Screen.height - boxH - 60, boxW, boxH);

            // Fondo de la caja de diálogo
            GUI.color = new Color(0.06f, 0.1f, 0.16f, 0.95f);
            GUI.Box(boxRect, GUIContent.none);
            GUI.color = Color.white;

            // Título / Nombre del NPC
            GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
            nameStyle.fontSize = 17;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.normal.textColor = new Color(0.2f, 0.9f, 1f);
            GUI.Label(new Rect(boxRect.x + 20, boxRect.y + 15, boxW - 40, 28), "🐧 " + npcName, nameStyle);

            // Contenido del diálogo
            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 14;
            textStyle.wordWrap = true;
            textStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(boxRect.x + 20, boxRect.y + 48, boxW - 40, 75), dialogueText, textStyle);

            // Indicador para cerrar
            GUIStyle closeStyle = new GUIStyle(GUI.skin.label);
            closeStyle.fontSize = 12;
            closeStyle.alignment = TextAnchor.MiddleRight;
            closeStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(boxRect.x + 20, boxRect.y + boxH - 28, boxW - 40, 22), "Presiona [E] para cerrar", closeStyle);
        }
    }
}
