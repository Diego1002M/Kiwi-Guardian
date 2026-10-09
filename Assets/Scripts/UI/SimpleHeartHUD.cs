using UnityEngine;
using Platformer2D;

/// <summary>
/// HUD en pantalla para mostrar los corazones de Kiwi, energía del Torbellino Acuático,
/// semillas de energía recolectadas, elemento activo y barra de vida del Jefe.
/// </summary>
public class SimpleHeartHUD : MonoBehaviour
{
    [Header("Texturas Pixel Art")]
    [SerializeField] private Texture2D fullHeartTex;
    [SerializeField] private Texture2D emptyHeartTex;
    [SerializeField] private Texture2D energySeedTex;

    private PlayerHealth playerHealth;
    private KiwiController kiwiController;
    private KiwiCombat kiwiCombat;
    public static int seedsCollected = 0;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();

        kiwiController = GetComponent<KiwiController>();
        if (kiwiController == null) kiwiController = FindAnyObjectByType<KiwiController>();

        kiwiCombat = GetComponent<KiwiCombat>();
        if (kiwiCombat == null) kiwiCombat = FindAnyObjectByType<KiwiCombat>();

        if (fullHeartTex == null) fullHeartTex = Resources.Load<Texture2D>("HeartFull");
        if (emptyHeartTex == null) emptyHeartTex = Resources.Load<Texture2D>("HeartEmpty");
        if (energySeedTex == null) energySeedTex = Resources.Load<Texture2D>("EnergySeed");
    }

    public void SetupTextures(Texture2D full, Texture2D empty, Texture2D seed)
    {
        fullHeartTex = full;
        emptyHeartTex = empty;
        energySeedTex = seed;
    }

    private void OnGUI()
    {
        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth == null) return;
        }

        if (kiwiController == null) kiwiController = FindAnyObjectByType<KiwiController>();
        if (kiwiCombat == null) kiwiCombat = FindAnyObjectByType<KiwiCombat>();

        int max = playerHealth.MaxHealth;
        int current = playerHealth.CurrentHealth;

        // 1. DIBUJAR CORAZONES DE SALUD
        GUI.color = new Color(0.06f, 0.1f, 0.16f, 0.85f);
        GUI.Box(new Rect(15, 12, max * 42 + 10, 48), GUIContent.none);
        GUI.color = Color.white;

        for (int i = 0; i < max; i++)
        {
            Rect heartRect = new Rect(20 + i * 42, 18, 36, 36);
            Texture2D tex = (i < current) ? fullHeartTex : emptyHeartTex;

            if (tex != null)
            {
                GUI.DrawTexture(heartRect, tex);
            }
            else
            {
                GUI.color = (i < current) ? Color.red : Color.gray;
                GUI.Label(heartRect, "♥");
                GUI.color = Color.white;
            }
        }

        // 2. DIBUJAR BARRA DE ENERGÍA (TORBELLINO ACUÁTICO - DASH)
        float dashCooldownRatio = (kiwiController != null) ? kiwiController.DashCooldownRatio : 0f;
        float dashEnergy = 1f - dashCooldownRatio;

        GUI.color = new Color(0.06f, 0.1f, 0.16f, 0.85f);
        GUI.Box(new Rect(15, 66, 175, 24), GUIContent.none);

        GUI.color = (dashEnergy >= 1f) ? new Color(0.2f, 0.85f, 1f, 0.9f) : new Color(0.15f, 0.45f, 0.7f, 0.7f);
        GUI.Box(new Rect(18, 69, 169 * dashEnergy, 18), GUIContent.none);

        GUIStyle dashTextStyle = new GUIStyle(GUI.skin.label);
        dashTextStyle.fontSize = 11;
        dashTextStyle.fontStyle = FontStyle.Bold;
        dashTextStyle.alignment = TextAnchor.MiddleCenter;
        dashTextStyle.normal.textColor = Color.white;
        GUI.color = Color.white;
        GUI.Label(new Rect(18, 67, 169, 22), (dashEnergy >= 1f) ? "🌀 TORBELLINO [Shift]" : "Recargando...", dashTextStyle);

        // 3. SEMILLAS DE ENERGÍA RECOLECTADAS
        if (energySeedTex != null)
        {
            GUI.color = new Color(0.06f, 0.1f, 0.16f, 0.85f);
            GUI.Box(new Rect(15, 96, 115, 36), GUIContent.none);
            GUI.color = Color.white;

            GUI.DrawTexture(new Rect(22, 100, 28, 28), energySeedTex);

            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 18;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.normal.textColor = new Color(0.1f, 0.95f, 0.65f);
            GUI.Label(new Rect(58, 102, 65, 28), "x " + seedsCollected, textStyle);
        }

        // 4. ELEMENTO ACTIVO DE KIWI
        if (kiwiCombat != null)
        {
            ElementType elem = kiwiCombat.CurrentElement;
            Color elemColor = ElementalHelpers.GetColor(elem);

            GUI.color = new Color(0.06f, 0.1f, 0.16f, 0.88f);
            GUI.Box(new Rect(15, 138, 175, 28), GUIContent.none);

            GUIStyle elemStyle = new GUIStyle(GUI.skin.label);
            elemStyle.fontSize = 12;
            elemStyle.fontStyle = FontStyle.Bold;
            elemStyle.alignment = TextAnchor.MiddleLeft;
            elemStyle.normal.textColor = elemColor;

            GUI.color = Color.white;
            GUI.Label(new Rect(24, 140, 160, 24), $"{ElementalHelpers.GetIcon(elem)}  [Q / 1-4]", elemStyle);
        }

        // 5. BARRA DE VIDA DEL JEFE
        PlasticWhaleBossAI whaleBoss = PlasticWhaleBossAI.ActiveBoss;
        ToxicHiveBossAI hiveBoss = ToxicHiveBossAI.ActiveBoss;

        if (whaleBoss != null && !whaleBoss.IsDead)
        {
            DrawBossBar(whaleBoss.transform.position, whaleBoss.CurrentHealth, whaleBoss.MaxHealth,
                "🐋 LA BALLENA DE PLÁSTICO (Espíritu de Agua Corrompido)",
                new Color(1f, 0.35f, 0.5f), new Color(0.85f, 0.25f, 0.75f, 0.95f));
        }
        else if (hiveBoss != null && !hiveBoss.IsDead)
        {
            DrawBossBar(hiveBoss.transform.position, hiveBoss.CurrentHealth, hiveBoss.MaxHealth,
                "🔥 LA COLMENA TÓXICA (Espíritu de Fuego Corrompido)",
                new Color(1f, 0.5f, 0.2f), new Color(1f, 0.35f, 0.1f, 0.95f));
        }
    }

    private void DrawBossBar(Vector3 bossPos, int currentHealth, int maxHealth, string title, Color titleColor, Color barColor)
    {
        float bossDist = Vector2.Distance(bossPos, playerHealth.transform.position);
        if (bossDist <= 24f)
        {
            float barWidth = Mathf.Min(450, Screen.width - 60);
            float barX = (Screen.width - barWidth) / 2f;
            float barY = 20;

            GUI.color = new Color(0.06f, 0.1f, 0.16f, 0.9f);
            GUI.Box(new Rect(barX - 6, barY - 6, barWidth + 12, 48), GUIContent.none);

            GUIStyle bossNameStyle = new GUIStyle(GUI.skin.label);
            bossNameStyle.fontSize = 13;
            bossNameStyle.fontStyle = FontStyle.Bold;
            bossNameStyle.alignment = TextAnchor.MiddleCenter;
            bossNameStyle.normal.textColor = titleColor;
            GUI.color = Color.white;
            GUI.Label(new Rect(barX, barY - 2, barWidth, 20), title, bossNameStyle);

            float fillRatio = Mathf.Clamp01((float)currentHealth / maxHealth);
            GUI.color = new Color(0.2f, 0.05f, 0.05f, 0.8f);
            GUI.Box(new Rect(barX, barY + 18, barWidth, 18), GUIContent.none);

            GUI.color = (fillRatio <= 0.4f) ? new Color(1f, 0.15f, 0.15f, 0.95f) : barColor;
            GUI.Box(new Rect(barX, barY + 18, barWidth * fillRatio, 18), GUIContent.none);
            GUI.color = Color.white;
        }

        // 6. GUÍA RÁPIDA DE CONTROLES
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 13;
        hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.75f);
        GUI.Label(new Rect(20, Screen.height - 42, 800, 30), "🎮 [A/D] Mover  |  [W] Salto  |  [W x2] Sobre-Salto  |  [Q / 1-4] Cambiar Elemento  |  [Shift] Torbellino  |  [J / X / Clic] Dagas", hintStyle);
    }
}
