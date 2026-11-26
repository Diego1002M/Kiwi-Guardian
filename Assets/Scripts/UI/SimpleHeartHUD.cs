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
        GuardianPutridoBossAI forestBoss = GuardianPutridoBossAI.ActiveBoss;
        CombustionColossusBossAI fireBoss = CombustionColossusBossAI.ActiveBoss;

        bool isBossInCombat = false;
        if (whaleBoss != null && !whaleBoss.IsDead)
        {
            isBossInCombat = DrawBossBar(whaleBoss.transform.position, whaleBoss.CurrentHealth, whaleBoss.MaxHealth,
                "🐋 LA BALLENA DE PLÁSTICO (Espíritu de Agua Corrompido)",
                new Color(0.2f, 0.85f, 1f), new Color(0.2f, 0.7f, 1f, 0.95f));
        }
        else if (forestBoss != null && !forestBoss.IsDead)
        {
            isBossInCombat = DrawBossBar(forestBoss.transform.position, forestBoss.CurrentHealth, forestBoss.MaxHealth,
                "🌲 EL GUARDIÁN PÚTRIDO (Espíritu de Tierra Corrompido)",
                new Color(0.3f, 1f, 0.45f), new Color(0.2f, 0.8f, 0.35f, 0.95f));
        }
        else if (fireBoss != null && !fireBoss.IsDead)
        {
            isBossInCombat = DrawBossBar(fireBoss.transform.position, fireBoss.CurrentHealth, fireBoss.MaxHealth,
                "🔥 EL COLOSO DE COMBUSTIÓN (Espíritu de Fuego Corrompido)",
                new Color(1f, 0.45f, 0.1f), new Color(1f, 0.3f, 0.05f, 0.95f));
        }
        else if (hiveBoss != null && !hiveBoss.IsDead)
        {
            isBossInCombat = DrawBossBar(hiveBoss.transform.position, hiveBoss.CurrentHealth, hiveBoss.MaxHealth,
                "💨 EL SUPERENJAMBRE TÓXICO (Espíritu de Viento Corrompido)",
                new Color(0.9f, 0.85f, 0.3f), new Color(0.85f, 0.75f, 0.1f, 0.95f));
        }

        // 6. PANEL DE OBJETIVOS Y PROGRESIÓN (ESQUINA SUPERIOR DERECHA)
        // Durante el combate activo contra el Jefe, ocultamos los objetivos generales para dar foco 100% cinematográfico al combate y evitar cualquier solapamiento
        if (!isBossInCombat)
        {
            float objW = 255;
            float objH = 88;
            float objX = Screen.width - objW - 15;
            float objY = 12;

            GUI.color = new Color(0.06f, 0.1f, 0.16f, 0.92f);
            GUI.Box(new Rect(objX, objY, objW, objH), GUIContent.none);
            GUI.color = Color.white;

            bool bWater = PlayerPrefs.GetInt("Boss_Water_Defeated", 0) == 1;
            bool bForest = PlayerPrefs.GetInt("Boss_Forest_Defeated", 0) == 1;
            bool bAir = PlayerPrefs.GetInt("Boss_Air_Defeated", 0) == 1;
            bool bEarth = PlayerPrefs.GetInt("Boss_Earth_Defeated", 0) == 1;
            int restoredCount = (bWater ? 1 : 0) + (bForest ? 1 : 0) + (bAir ? 1 : 0) + (bEarth ? 1 : 0);

            GUIStyle objTitle = new GUIStyle(GUI.skin.label);
            objTitle.fontSize = 11;
            objTitle.fontStyle = FontStyle.Bold;
            objTitle.normal.textColor = new Color(0.4f, 0.9f, 1f);
            GUI.Label(new Rect(objX + 10, objY + 4, objW - 20, 18), $"🌱 ÁRBOL DE LA VIDA: {restoredCount}/4", objTitle);

            string wIcon = bWater ? "<color=#3df>💧</color>" : "<color=#555>💧</color>";
            string fIcon = bForest ? "<color=#4f6>🌿</color>" : "<color=#555>🌿</color>";
            string aIcon = bAir ? "<color=#fff>💨</color>" : "<color=#555>💨</color>";
            string eIcon = bEarth ? "<color=#f82>🔥</color>" : "<color=#555>🔥</color>";

            GUIStyle crestStyle = new GUIStyle(GUI.skin.label);
            crestStyle.fontSize = 15;
            GUI.Label(new Rect(objX + 10, objY + 22, objW - 20, 24), $"{wIcon} Agua  {fIcon} Tierra  {aIcon} Aire  {eIcon} Fuego", crestStyle);

            GUIStyle subObj = new GUIStyle(GUI.skin.label);
            subObj.fontSize = 10;
            subObj.normal.textColor = new Color(0.85f, 0.85f, 0.85f, 0.8f);
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string goalText = currentScene == "ParamoToxico" ? "Entra a un portal para purificarlo" : "Purifica el Altar y vence al Jefe";
            GUI.Label(new Rect(objX + 10, objY + 46, objW - 20, 18), $"🎯 {goalText}", subObj);

            // Recordatorio de Mapa Mundial integrado limpiamente sin solapamiento
            GUIStyle mapStyle = new GUIStyle(GUI.skin.label);
            mapStyle.fontSize = 10;
            mapStyle.fontStyle = FontStyle.Bold;
            mapStyle.normal.textColor = new Color(0.35f, 0.95f, 1f, 0.95f);
            GUI.Label(new Rect(objX + 10, objY + 66, objW - 20, 18), "🗺️ Mapa Mundial: Pulsa [M]", mapStyle);
        }

        // 7. GUÍA RÁPIDA DE CONTROLES (PARTE INFERIOR)
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 12;
        hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.75f);
        GUI.Label(new Rect(20, Screen.height - 38, 920, 30), "🎮 [A/D] Mover  |  [W] Salto  |  [W en Pared] Salto en Pared  |  [W x2] Sobre-Salto  |  [Shift] Torbellino  |  [Q / 1-4] Elementos  |  [J / Clic] Dagas", hintStyle);
    }

    private bool DrawBossBar(Vector3 bossPos, int currentHealth, int maxHealth, string title, Color titleColor, Color barColor)
    {
        float bossDist = Vector2.Distance(bossPos, playerHealth.transform.position);
        if (bossDist <= 26f)
        {
            float barWidth = Mathf.Clamp(Screen.width - 280, 260, 480);
            float barX = (Screen.width - barWidth) / 2f;
            float barY = 16;

            GUI.color = new Color(0.06f, 0.1f, 0.16f, 0.94f);
            GUI.Box(new Rect(barX - 8, barY - 6, barWidth + 16, 52), GUIContent.none);

            GUIStyle bossNameStyle = new GUIStyle(GUI.skin.label);
            bossNameStyle.fontSize = 12;
            bossNameStyle.fontStyle = FontStyle.Bold;
            bossNameStyle.alignment = TextAnchor.MiddleCenter;
            bossNameStyle.normal.textColor = titleColor;
            GUI.color = Color.white;
            GUI.Label(new Rect(barX, barY - 2, barWidth, 20), title, bossNameStyle);

            float fillRatio = Mathf.Clamp01((float)currentHealth / maxHealth);
            GUI.color = new Color(0.2f, 0.05f, 0.05f, 0.85f);
            GUI.Box(new Rect(barX, barY + 20, barWidth, 18), GUIContent.none);

            GUI.color = (fillRatio <= 0.4f) ? new Color(1f, 0.2f, 0.2f, 0.95f) : barColor;
            GUI.Box(new Rect(barX, barY + 20, barWidth * fillRatio, 18), GUIContent.none);

            // Valor numérico de salud del jefe
            GUIStyle healthNumberStyle = new GUIStyle(GUI.skin.label);
            healthNumberStyle.fontSize = 11;
            healthNumberStyle.fontStyle = FontStyle.Bold;
            healthNumberStyle.alignment = TextAnchor.MiddleCenter;
            healthNumberStyle.normal.textColor = Color.white;
            GUI.color = Color.white;
            GUI.Label(new Rect(barX, barY + 20, barWidth, 18), $"{currentHealth} / {maxHealth}", healthNumberStyle);

            return true;
        }
        return false;
    }
}
