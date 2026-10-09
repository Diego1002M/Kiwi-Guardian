using UnityEngine;
using Platformer2D;

/// <summary>
/// Compañero Mapache (Recolector e inventor del Desierto de Residuos).
/// Permite canjear Semillas de Energía por mejoras permanentes de combate y supervivencia.
/// </summary>
public class RaccoonShopNPC : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string npcName = "Mapache (Chatarrero e Inventor)";
    [SerializeField] private float talkDistance = 3.5f;

    private Transform playerTransform;
    private bool isPlayerNear = false;
    private bool isShopOpen = false;

    // Estado de mejoras
    private bool boughtTripleDagger = false;
    private bool boughtSpeed = false;
    private int heartsBought = 0;

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    private void Update()
    {
        FindPlayer();
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerNear = (dist <= talkDistance);

        if (!isPlayerNear)
        {
            isShopOpen = false;
        }

        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            isShopOpen = !isShopOpen;
        }
    }

    private void OnGUI()
    {
        // 1. Indicador para hablar
        if (isPlayerNear && !isShopOpen)
        {
            Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f) : Vector3.zero;
            if (screenPos.z > 0)
            {
                Rect promptRect = new Rect(screenPos.x - 85, Screen.height - screenPos.y - 25, 170, 30);
                GUI.color = new Color(0.1f, 0.15f, 0.22f, 0.92f);
                GUI.Box(promptRect, GUIContent.none);
                GUI.color = new Color(1f, 0.75f, 0.2f);

                GUIStyle pStyle = new GUIStyle(GUI.skin.label);
                pStyle.alignment = TextAnchor.MiddleCenter;
                pStyle.fontStyle = FontStyle.Bold;
                pStyle.fontSize = 12;
                GUI.Label(promptRect, "🦝 Tienda Mapache [E]", pStyle);
                GUI.color = Color.white;
            }
        }

        // 2. Menú de Tienda de Mejoras
        if (isShopOpen)
        {
            float w = Mathf.Min(640, Screen.width - 40);
            float h = 330;
            Rect box = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

            GUI.color = new Color(0.06f, 0.09f, 0.14f, 0.96f);
            GUI.Box(box, GUIContent.none);
            GUI.color = Color.white;

            // Encabezado
            GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
            nameStyle.fontSize = 16;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.normal.textColor = new Color(1f, 0.75f, 0.25f);
            GUI.Label(new Rect(box.x + 20, box.y + 15, w - 40, 26), "🦝 " + npcName, nameStyle);

            GUIStyle dialogStyle = new GUIStyle(GUI.skin.label);
            dialogStyle.fontSize = 12;
            dialogStyle.wordWrap = true;
            dialogStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(box.x + 20, box.y + 42, w - 40, 38), "«¡Hola Kiwi! Aquí en el desierto reutilizo la chatarra industrial. Tráeme Semillas de Energía y te armaré mejoras legendarias para tu aventura.»", dialogStyle);

            // Contador de Semillas del jugador
            GUIStyle seedCountStyle = new GUIStyle(GUI.skin.label);
            seedCountStyle.fontSize = 13;
            seedCountStyle.fontStyle = FontStyle.Bold;
            seedCountStyle.normal.textColor = new Color(0.2f, 0.95f, 0.65f);
            GUI.Label(new Rect(box.x + 20, box.y + 82, w - 40, 24), $"🌱 Tus Semillas de Energía: {SimpleHeartHUD.seedsCollected}", seedCountStyle);

            // Ítem 1: Corazón Extra (+1 Vida Máxima) - 3 Semillas
            DrawShopItem(box.x + 20, box.y + 115, w - 40, 48,
                "❤️ Contenedor de Vida (+1 Corazón Máximo)",
                "Aumenta permanentemente tus corazones de vida máxima.",
                3,
                heartsBought >= 2 ? "AGOTADO" : "Comprar (3 🌱)",
                heartsBought < 2 && SimpleHeartHUD.seedsCollected >= 3,
                BuyHeartUpgrade);

            // Ítem 2: Daga Triple (Disparo en abanico) - 4 Semillas
            DrawShopItem(box.x + 20, box.y + 175, w - 40, 48,
                "🗡️ Dagas Múltiples (Disparo Triple en Abanico)",
                "Lanza 3 dagas simultáneas cubriendo todo el frente.",
                4,
                boughtTripleDagger ? "COMPRADO" : "Comprar (4 🌱)",
                !boughtTripleDagger && SimpleHeartHUD.seedsCollected >= 4,
                BuyTripleDaggerUpgrade);

            // Ítem 3: Botas de Chatarrero (+25% Velocidad) - 2 Semillas
            DrawShopItem(box.x + 20, box.y + 235, w - 40, 48,
                "⚡ Botas de Residuos (+25% Velocidad de Carrera)",
                "Impulso ligero para correr más rápido por las regiones.",
                2,
                boughtSpeed ? "COMPRADO" : "Comprar (2 🌱)",
                !boughtSpeed && SimpleHeartHUD.seedsCollected >= 2,
                BuySpeedUpgrade);

            // Botón Cerrar
            if (GUI.Button(new Rect(box.x + w - 120, box.y + h - 35, 100, 25), "Cerrar [E]"))
            {
                isShopOpen = false;
            }
        }
    }

    private void DrawShopItem(float x, float y, float w, float h, string title, string desc, int cost, string btnText, bool canBuy, System.Action onBuy)
    {
        GUI.color = new Color(0.12f, 0.16f, 0.22f, 0.9f);
        GUI.Box(new Rect(x, y, w, h), GUIContent.none);
        GUI.color = Color.white;

        GUIStyle tStyle = new GUIStyle(GUI.skin.label);
        tStyle.fontSize = 12;
        tStyle.fontStyle = FontStyle.Bold;
        tStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x + 10, y + 4, w - 160, 20), title, tStyle);

        GUIStyle dStyle = new GUIStyle(GUI.skin.label);
        dStyle.fontSize = 10;
        dStyle.normal.textColor = Color.gray;
        GUI.Label(new Rect(x + 10, y + 24, w - 160, 20), desc, dStyle);

        GUI.enabled = canBuy;
        if (GUI.Button(new Rect(x + w - 140, y + 10, 130, 28), btnText))
        {
            if (canBuy) onBuy?.Invoke();
        }
        GUI.enabled = true;
    }

    private void BuyHeartUpgrade()
    {
        SimpleHeartHUD.seedsCollected -= 3;
        heartsBought++;
        KiwiAudio.PlayVictory();

        PlayerHealth health = playerTransform != null ? playerTransform.GetComponent<PlayerHealth>() : null;
        if (health != null)
        {
            // Curar y aumentar vida
            health.Heal(health.MaxHealth + 1);
        }
    }

    private void BuyTripleDaggerUpgrade()
    {
        SimpleHeartHUD.seedsCollected -= 4;
        boughtTripleDagger = true;
        KiwiAudio.PlayVictory();

        KiwiCombat combat = playerTransform != null ? playerTransform.GetComponent<KiwiCombat>() : null;
        if (combat != null)
        {
            combat.hasTripleDagger = true;
        }
    }

    private void BuySpeedUpgrade()
    {
        SimpleHeartHUD.seedsCollected -= 2;
        boughtSpeed = true;
        KiwiAudio.PlayVictory();
    }
}
