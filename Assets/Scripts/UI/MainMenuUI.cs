using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador de la Pantalla de Título Principal de Kiwi: El Guardián de la Última Semilla.
/// Presenta menú interactivo, selección de región, panel de controles y transición al prólogo.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Recursos Visuales")]
    [SerializeField] private Texture2D titleLogoTex;
    [SerializeField] private Texture2D giantSeedTex;
    [SerializeField] private Texture2D oceanBgTex;

    private enum MenuState { Main, RegionSelect, Controls, InPrologue }
    private MenuState currentState = MenuState.Main;

    private float pulseTimer = 0f;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle modalStyle;
    private GUIStyle headerStyle;

    private void Start()
    {
        if (titleLogoTex == null) titleLogoTex = Resources.Load<Texture2D>("Assets/Sprites/TitleLogo.png");
        if (giantSeedTex == null) giantSeedTex = Resources.Load<Texture2D>("Assets/Sprites/GiantSeed.png");
        
        #if UNITY_EDITOR
        if (titleLogoTex == null)
            titleLogoTex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/TitleLogo.png");
        if (giantSeedTex == null)
            giantSeedTex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/GiantSeed.png");
        if (oceanBgTex == null)
            oceanBgTex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/OceanBackground.png");
        #endif
    }

    private void Update()
    {
        pulseTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState != MenuState.Main && currentState != MenuState.InPrologue)
            {
                currentState = MenuState.Main;
                KiwiAudio.PlayJump();
            }
        }
    }

    private void OnGUI()
    {
        if (currentState == MenuState.InPrologue) return;

        InitStyles();

        // 1. Fondo atmosférico
        if (oceanBgTex != null)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), oceanBgTex, ScaleMode.ScaleAndCrop);
        }
        else
        {
            GUI.color = new Color(0.08f, 0.06f, 0.14f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // Tinte de viñeta oscura para enfocar el menú
        GUI.color = new Color(0.03f, 0.05f, 0.1f, 0.65f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // 2. Semilla Gigante Palpitante en el Centro
        if (giantSeedTex != null)
        {
            float seedScale = 1.0f + Mathf.Sin(pulseTimer * 2.8f) * 0.08f;
            float seedW = 120 * seedScale;
            float seedH = 120 * seedScale;
            float seedX = (Screen.width - seedW) / 2f;
            float seedY = Screen.height * 0.18f - (seedH - 120) / 2f;

            // Halo dorado
            GUI.color = new Color(1f, 0.85f, 0.2f, 0.25f + Mathf.Sin(pulseTimer * 3f) * 0.1f);
            GUI.DrawTexture(new Rect(seedX - 25, seedY - 25, seedW + 50, seedH + 50), giantSeedTex);

            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(seedX, seedY, seedW, seedH), giantSeedTex);
        }

        // 3. Logo del Juego
        if (titleLogoTex != null)
        {
            float logoW = Mathf.Min(480, Screen.width - 40);
            float logoH = logoW * (56f / 192f);
            float logoX = (Screen.width - logoW) / 2f;
            float logoY = Screen.height * 0.36f;
            GUI.DrawTexture(new Rect(logoX, logoY, logoW, logoH), titleLogoTex);
        }
        else
        {
            GUI.Label(new Rect(0, Screen.height * 0.34f, Screen.width, 50), "KIWI", titleStyle);
            GUI.Label(new Rect(0, Screen.height * 0.40f, Screen.width, 30), "EL GUARDIÁN DE LA ÚLTIMA SEMILLA", headerStyle);
        }

        // Subtítulo místico
        GUIStyle subStyle = new GUIStyle(GUI.skin.label);
        subStyle.fontSize = 14;
        subStyle.alignment = TextAnchor.MiddleCenter;
        subStyle.normal.textColor = new Color(0.3f, 0.95f, 1f, 0.85f);
        GUI.Label(new Rect(0, Screen.height * 0.50f, Screen.width, 26), "🌱 Aventura Metroidvania de Purificación Elemental 🌱", subStyle);

        // 4. Estados del Menú
        switch (currentState)
        {
            case MenuState.Main:
                DrawMainMenuButtons();
                break;
            case MenuState.RegionSelect:
                DrawRegionSelectModal();
                break;
            case MenuState.Controls:
                DrawControlsModal();
                break;
        }

        // Pie de página
        GUIStyle footerStyle = new GUIStyle(GUI.skin.label);
        footerStyle.fontSize = 11;
        footerStyle.alignment = TextAnchor.MiddleCenter;
        footerStyle.normal.textColor = new Color(1f, 1f, 1f, 0.45f);
        GUI.Label(new Rect(0, Screen.height - 24, Screen.width, 20), "Kiwi: Guardian of the Last Seed • Creado con Unity 6", footerStyle);
    }

    private void DrawMainMenuButtons()
    {
        float btnW = 260;
        float btnH = 46;
        float btnX = (Screen.width - btnW) / 2f;
        float startY = Screen.height * 0.57f;
        float spacing = 54;

        if (GUI.Button(new Rect(btnX, startY, btnW, btnH), "🌟  INICIAR AVENTURA", buttonStyle))
        {
            KiwiAudio.PlayDoubleJump();
            StartPrologue();
        }

        if (GUI.Button(new Rect(btnX, startY + spacing, btnW, btnH), "🗺️  SELECCIONAR REGIÓN", buttonStyle))
        {
            KiwiAudio.PlayJump();
            currentState = MenuState.RegionSelect;
        }

        if (GUI.Button(new Rect(btnX, startY + spacing * 2, btnW, btnH), "🎮  CONTROLES", buttonStyle))
        {
            KiwiAudio.PlayJump();
            currentState = MenuState.Controls;
        }

        if (GUI.Button(new Rect(btnX, startY + spacing * 3, btnW, btnH), "❌  SALIR", buttonStyle))
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }

    private void DrawRegionSelectModal()
    {
        float mw = Mathf.Min(480, Screen.width - 40);
        float mh = 390;
        float mx = (Screen.width - mw) / 2f;
        float my = (Screen.height - mh) / 2f;

        GUI.color = new Color(0.08f, 0.12f, 0.18f, 0.96f);
        GUI.Box(new Rect(mx, my, mw, mh), GUIContent.none, modalStyle);
        GUI.color = Color.white;

        GUI.Label(new Rect(mx, my + 15, mw, 30), "🗺️ SELECCIONAR REGIÓN", headerStyle);

        float bw = mw - 50;
        float bh = 42;
        float bx = mx + 25;
        float by = my + 55;
        float step = 50;

        if (GUI.Button(new Rect(bx, by, bw, bh), "🌊 1. Mar de Plástico (Agua / Ballena Boss)", buttonStyle))
        {
            LoadSceneByName("MarDePlastico");
        }

        if (GUI.Button(new Rect(bx, by + step, bw, bh), "☠️ 2. Páramo Tóxico (Hub Central)", buttonStyle))
        {
            LoadSceneByName("ParamoToxico");
        }

        if (GUI.Button(new Rect(bx, by + step * 2, bw, bh), "💨 3. Ciudad de Humo (Aire / Búho Guía)", buttonStyle))
        {
            LoadSceneByName("CiudadDeHumo");
        }

        if (GUI.Button(new Rect(bx, by + step * 3, bw, bh), "🔥 4. Desierto de Residuos (Fuego / Colmena Boss)", buttonStyle))
        {
            LoadSceneByName("DesiertoDeResiduos");
        }

        if (GUI.Button(new Rect(bx, by + step * 4, bw, bh), "🌱 5. Bosque Marchito (Tierra / Zorro)", buttonStyle))
        {
            LoadSceneByName("BosqueMarchito");
        }

        if (GUI.Button(new Rect(bx + bw * 0.25f, by + step * 5 + 8, bw * 0.5f, 34), "⬅️ Volver", buttonStyle))
        {
            KiwiAudio.PlayJump();
            currentState = MenuState.Main;
        }
    }

    private void DrawControlsModal()
    {
        float mw = Mathf.Min(560, Screen.width - 40);
        float mh = 430;
        float mx = (Screen.width - mw) / 2f;
        float my = (Screen.height - mh) / 2f;

        GUI.color = new Color(0.08f, 0.12f, 0.18f, 0.96f);
        GUI.Box(new Rect(mx, my, mw, mh), GUIContent.none, modalStyle);
        GUI.color = Color.white;

        GUI.Label(new Rect(mx, my + 15, mw, 30), "🎮 GUÍA DE CONTROLES", headerStyle);

        string[] controls = {
            "A / D  o  ← / →", "Correr (con inclinación de inercia)",
            "W  o  Espacio", "Salto Fijo calibrado (~2.6m)",
            "W  (en el aire)", "¡SOBRE-SALTO! Impulso extra con onda expansiva",
            "Shift", "Torbellino Dash (esquiva rápida con estela)",
            "Q  o  1, 2, 3, 4", "Cambio de Elemento: Agua, Fuego, Aire, Tierra",
            "J  o  X  o  Clic", "Lanzar Dagas Elementales",
            "E", "Interactuar (Hablar con Compañeros, Tienda, Tótem)",
            "M", "Abrir Mapa Mundial Interconectado"
        };

        GUIStyle rowKeyStyle = new GUIStyle(GUI.skin.label);
        rowKeyStyle.fontSize = 13;
        rowKeyStyle.fontStyle = FontStyle.Bold;
        rowKeyStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

        GUIStyle rowDescStyle = new GUIStyle(GUI.skin.label);
        rowDescStyle.fontSize = 13;
        rowDescStyle.normal.textColor = Color.white;

        float cy = my + 55;
        for (int i = 0; i < controls.Length; i += 2)
        {
            GUI.Label(new Rect(mx + 35, cy, 180, 24), controls[i], rowKeyStyle);
            GUI.Label(new Rect(mx + 200, cy, mw - 230, 24), controls[i + 1], rowDescStyle);
            cy += 32;
        }

        if (GUI.Button(new Rect(mx + (mw - 160) / 2f, my + mh - 50, 160, 36), "¡Entendido!", buttonStyle))
        {
            KiwiAudio.PlayJump();
            currentState = MenuState.Main;
        }
    }

    private void StartPrologue()
    {
        currentState = MenuState.InPrologue;
        PrologueCutscene cutscene = FindObjectOfType<PrologueCutscene>();
        if (cutscene == null)
        {
            GameObject go = new GameObject("--- PROLOGUE CUTSCENE ---");
            cutscene = go.AddComponent<PrologueCutscene>();
        }
        cutscene.StartCutscene(() =>
        {
            LoadSceneByName("MarDePlastico");
        });
    }

    private void LoadSceneByName(string sceneName)
    {
        KiwiAudio.PlayVictory();
        SceneManager.LoadScene(sceneName);
    }

    private void InitStyles()
    {
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.hover.textColor = new Color(1f, 0.9f, 0.3f);
        }

        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 17;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = new Color(0.2f, 0.9f, 1f);
        }

        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 32;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        }

        if (modalStyle == null)
        {
            modalStyle = new GUIStyle(GUI.skin.box);
        }
    }
}
