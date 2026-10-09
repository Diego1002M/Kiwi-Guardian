using UnityEngine;
using Platformer2D;

/// <summary>
/// Tótem de descanso sagrado (estilo banco de Hollow Knight / tótems de la Última Semilla).
/// Permite a Kiwi descansar con [E], recuperar todos sus corazones y registrar su punto de reaparición.
/// </summary>
public class RestTotem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string totemName = "Tótem del Equilibrio Elemental";
    [TextArea(2, 4)]
    [SerializeField] private string restLoreText = "La brisa acaricia las hojas de la Última Semilla. Tu salud y energía han sido completamente restauradas.";

    private Transform playerTransform;
    private bool isPlayerNear = false;
    private bool isResting = false;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

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
        isPlayerNear = (dist <= 3.2f);

        if (!isPlayerNear)
        {
            isResting = false;
        }

        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            Rest();
        }
    }

    private void Rest()
    {
        isResting = true;
        KiwiAudio.PlayVictory();

        // Restaurar salud de Kiwi
        if (playerTransform != null)
        {
            PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.Heal(health.MaxHealth);
            }
        }
    }

    private void OnGUI()
    {
        if (isPlayerNear && !isResting)
        {
            Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f) : Vector3.zero;
            if (screenPos.z > 0)
            {
                Rect r = new Rect(screenPos.x - 75, Screen.height - screenPos.y - 25, 150, 30);
                GUI.color = new Color(0.06f, 0.12f, 0.18f, 0.9f);
                GUI.Box(r, GUIContent.none);
                GUI.color = new Color(0.2f, 0.95f, 0.65f);

                GUIStyle s = new GUIStyle(GUI.skin.label);
                s.alignment = TextAnchor.MiddleCenter;
                s.fontStyle = FontStyle.Bold;
                s.fontSize = 12;
                GUI.Label(r, "🌿 Descansar [E]", s);
                GUI.color = Color.white;
            }
        }

        if (isResting)
        {
            float w = Mathf.Min(550, Screen.width - 40);
            float h = 110;
            Rect box = new Rect((Screen.width - w) / 2f, Screen.height - h - 55, w, h);

            GUI.color = new Color(0.06f, 0.1f, 0.16f, 0.95f);
            GUI.Box(box, GUIContent.none);
            GUI.color = Color.white;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 14;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.2f, 0.9f, 0.6f);
            GUI.Label(new Rect(box.x + 15, box.y + 10, w - 30, 24), "🌿 " + totemName + " (Punto de Guardado)", titleStyle);

            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 13;
            textStyle.wordWrap = true;
            textStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(box.x + 15, box.y + 36, w - 30, 40), restLoreText, textStyle);

            GUIStyle closeStyle = new GUIStyle(GUI.skin.label);
            closeStyle.fontSize = 11;
            closeStyle.alignment = TextAnchor.MiddleRight;
            closeStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(box.x + 15, box.y + h - 24, w - 30, 20), "¡Salud al 100%! Presiona [E] para levantarte", closeStyle);
        }
    }
}
