#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Platformer2D;

/// <summary>
/// Generador del Mundo Metroidvania de Kiwi:
/// Construye el Páramo Tóxico (Hub Central) y la Ciudad de Humo (Región 2),
/// registrando todas las escenas en Build Settings sin modificar ni borrar el Mar de Plástico.
/// </summary>
public static class WorldBuilder
{
    [MenuItem("Kiwi/🌟 Montar Menú Principal y Prólogo", priority = -1)]
    public static void SetupMainMenu() => SetupMainMenu(true);
    public static void SetupMainMenu(bool showDialog)
    {
        PixelArtGenerator.GenerateAllSprites();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Cámara 2D
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(0f, 0f, -10f);

        Camera cam = cameraObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
        cameraObj.AddComponent<AudioListener>();

        // Partículas místicas de fondo
        cameraObj.AddComponent<AtmosphericParticles>();

        // 2. Controlador de Menú y Prólogo
        GameObject menuObj = new GameObject("--- MAIN MENU MANAGER ---");
        System.Type menuType = System.Type.GetType("MainMenuUI, Assembly-CSharp") ?? System.Type.GetType("MainMenuUI");
        if (menuType != null) menuObj.AddComponent(menuType);
        System.Type cutsceneType = System.Type.GetType("PrologueCutscene, Assembly-CSharp") ?? System.Type.GetType("PrologueCutscene");
        if (cutsceneType != null) menuObj.AddComponent(cutsceneType);

        // 3. Guardar escena
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        string scenePath = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorSceneManager.OpenScene(scenePath);

        RegisterAllScenesInBuildSettings();

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "🌟 Pantalla de Inicio y Prólogo Creados",
                "¡La Pantalla de Título Principal y la Cinemática del Prólogo están listas!\n\n" +
                "• Iniciar Aventura: Reproduce el prólogo cinematográfico ilustrado y te lleva al Mar de Plástico.\n" +
                "• Seleccionar Región: Salta directamente a cualquiera de los 5 mundos.\n" +
                "• Controles: Consulta el mapa completo de teclas.\n" +
                "• Nuevo diseño de Kiwi: Gato naranja cálido con capucha verde azulada y mochila con cristal.\n\n" +
                "¡Pulsa Play (▶) para disfrutar de la experiencia!",
                "¡A jugar!"
            );
        }
    }

    [MenuItem("Kiwi/🌟 Montar Todo el Mundo (Todas las Regiones)", priority = 0)]
    public static void SetupAllWorld()
    {
        PixelArtGenerator.GenerateAllSprites();
        AutoSetupScene.SetupScene(false);
        SetupParamoToxico(false);
        SetupCiudadDeHumo(false);
        SetupDesiertoDeResiduos(false);
        SetupBosqueMarchito(false);
        SetupMainMenu(false);
        RegisterAllScenesInBuildSettings();

        string menuPath = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.OpenScene(menuPath);

        EditorUtility.DisplayDialog(
            "🌟 ¡Mundo Metroidvania y Prólogo Montados!",
            "¡Todas las regiones de Kiwi, la Pantalla de Inicio y el Prólogo han sido generados!\n\n" +
            "0. 🌟 Menú Principal y Prólogo Cinemático Ilustrado.\n" +
            "1. 🌊 Mar de Plástico (Agua) — Jefe Ballena con tsunami, furia y 25 HP.\n" +
            "2. ☠️ Páramo Tóxico (Hub Central) — Conecta las 4 regiones, Tótem de curación.\n" +
            "3. 💨 Ciudad de Humo (Aire) — Búho Guía, Nubes Tóxicas y chimeneas.\n" +
            "4. 🔥 Desierto de Residuos (Fuego) — Jefe Colmena con 28 HP y Mapache Inventor.\n" +
            "5. 🌱 Bosque Marchito (Tierra) — Zorro Compañero, Espectros y Muros de raíces.\n\n" +
            "¡Pulsa Play (▶) para comenzar tu aventura desde el Menú Principal!",
            "¡A jugar!"
        );
    }

    [MenuItem("Kiwi/☠️ Montar Páramo Tóxico (Hub Central)", priority = 10)]
    public static void SetupParamoToxico() => SetupParamoToxico(true);
    public static void SetupParamoToxico(bool showDialog)
    {
        PixelArtGenerator.GenerateAllSprites();

        Sprite kiwiSprite = LoadSprite("Assets/Sprites/KiwiCat.png");
        Sprite daggerSprite = LoadSprite("Assets/Sprites/KiwiDagger.png");
        Sprite platformSprite = LoadSprite("Assets/Sprites/TrashPlatform.png");
        Sprite totemSprite = LoadSprite("Assets/Sprites/RestTotem.png");
        Sprite portalSprite = LoadSprite("Assets/Sprites/PortalVortex.png");
        Sprite seedSprite = LoadSprite("Assets/Sprites/EnergySeed.png");

        Texture2D heartFullTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/HeartFull.png");
        Texture2D heartEmptyTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/HeartEmpty.png");
        Texture2D seedTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/EnergySeed.png");

        GameObject daggerPrefab = CreateDaggerPrefab(daggerSprite);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Cámara
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(0f, 1f, -10f);

        Camera cam = cameraObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.08f, 0.12f); // Tierra baldía mística
        cameraObj.AddComponent<AudioListener>();

        CameraFollow2D camFollow = cameraObj.AddComponent<CameraFollow2D>();
        cameraObj.AddComponent<AtmosphericParticles>();
        cameraObj.AddComponent<CombatFeedback>();

        // Fondo con Parallax para el Páramo
        Sprite smogBg = LoadSprite("Assets/Sprites/SmogBackground.png");
        GameObject bgObj = new GameObject("Fondo Paramo");
        bgObj.transform.position = new Vector3(0f, 1f, 10f);

        SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = smogBg;
        bgSr.color = new Color(0.7f, 0.5f, 0.85f, 0.85f); // Tinte místico violeta
        bgSr.sortingOrder = -50;
        bgSr.drawMode = SpriteDrawMode.Tiled;
        bgSr.size = new Vector2(100f, 13.5f);
        bgObj.AddComponent<ParallaxBackground>();

        GameObject envParent = new GameObject("--- PÁRAMO TÓXICO (HUB CENTRAL) ---");

        // Plataforma Central del Hub
        CreatePlatform("Plataforma Hub Central", new Vector3(0f, -2.5f, 0f), new Vector3(28f, 1.4f, 1f), Color.white, platformSprite, envParent.transform);

        // Tótem de Descanso (Centro del Hub)
        CreateRestTotem("Tótem Central del Páramo", new Vector3(0f, -1.05f, 0f), totemSprite, envParent.transform);

        // 1. PORTAL OESTE: Mar de Plástico (Región de Agua)
        CreatePortal("Portal Mar de Plástico", new Vector3(-10f, -0.8f, 0f), portalSprite, "MarDePlastico", "🌊 Mar de Plástico (Agua)", new Color(0.2f, 0.85f, 1f), envParent.transform);

        // 2. PORTAL NOROESTE: Ciudad de Humo (Región de Aire)
        CreatePortal("Portal Ciudad de Humo", new Vector3(-4.5f, -0.8f, 0f), portalSprite, "CiudadDeHumo", "💨 Ciudad de Humo (Aire)", new Color(0.85f, 0.9f, 0.95f), envParent.transform);

        // 3. PORTAL SURESTE: Desierto de Residuos (Región de Fuego)
        CreatePortal("Portal Desierto de Residuos", new Vector3(4.5f, -0.8f, 0f), portalSprite, "DesiertoDeResiduos", "🔥 Desierto de Residuos (Fuego)", new Color(1f, 0.45f, 0.1f), envParent.transform);

        // 4. PORTAL ESTE: Bosque Marchito (Región de Tierra)
        CreatePortal("Portal Bosque Marchito", new Vector3(10f, -0.8f, 0f), portalSprite, "BosqueMarchito", "🌱 Bosque Marchito (Tierra)", new Color(0.3f, 0.9f, 0.35f), envParent.transform);

        // Plataforma Alta: Camino al Santuario Natural
        CreatePlatform("Plataforma Camino Santuario", new Vector3(0f, 2.5f, 0f), new Vector3(10f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateSeedPickup(new Vector3(0f, 3.8f, 0f), seedSprite, envParent.transform);

        // Paredes delimitadoras
        CreatePlatform("Pared Izquierda", new Vector3(-14.5f, 2f, 0f), new Vector3(1.2f, 14f, 1f), Color.gray, platformSprite, envParent.transform);
        CreatePlatform("Pared Derecha", new Vector3(14.5f, 2f, 0f), new Vector3(1.2f, 14f, 1f), Color.gray, platformSprite, envParent.transform);

        // Kiwi Protagonista
        GameObject kiwi = CreateKiwi(new Vector3(-4f, -1.2f, 0f), kiwiSprite, daggerPrefab, heartFullTex, heartEmptyTex, seedTex);
        camFollow.SetTarget(kiwi.transform);

        // Guardar escena
        string path = "Assets/Scenes/ParamoToxico.unity";
        EditorSceneManager.SaveScene(scene, path);
        EditorSceneManager.OpenScene(path);

        RegisterAllScenesInBuildSettings();

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "☠️ Páramo Tóxico (Hub Central) Creado",
                "¡El Hub Central de las 4 regiones ha sido creado!\n\n" +
                "• Portal Oeste: Conecta directamente al Mar de Plástico.\n" +
                "• Portal Noroeste: Conecta a la Ciudad de Humo.\n" +
                "• Portal Sureste: Conecta al Desierto de Residuos.\n" +
                "• Portal Este: Conecta al Bosque Marchito.\n" +
                "• Tótem de Descanso: Descansa y cura tus vidas con [E].\n" +
                "• Mapa Mundial: Pulsa [M] para abrir el mapa de nodos estilo Hollow Knight.\n\n" +
                "¡Pulsa Play (▶) para explorar!",
                "¡Genial!"
            );
        }
    }

    [MenuItem("Kiwi/💨 Montar Región 2: Ciudad de Humo", priority = 20)]
    public static void SetupCiudadDeHumo() => SetupCiudadDeHumo(true);
    public static void SetupCiudadDeHumo(bool showDialog)
    {
        PixelArtGenerator.GenerateAllSprites();

        Sprite kiwiSprite = LoadSprite("Assets/Sprites/KiwiCat.png");
        Sprite daggerSprite = LoadSprite("Assets/Sprites/KiwiDagger.png");
        Sprite platformSprite = LoadSprite("Assets/Sprites/FactoryPlatform.png");
        Sprite bgSprite = LoadSprite("Assets/Sprites/SmogBackground.png");
        Sprite owlSprite = LoadSprite("Assets/Sprites/OwlCompanion.png");
        Sprite cloudSprite = LoadSprite("Assets/Sprites/ToxicCloud.png");
        Sprite seedSprite = LoadSprite("Assets/Sprites/EnergySeed.png");
        Sprite portalSprite = LoadSprite("Assets/Sprites/PortalVortex.png");

        Texture2D heartFullTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/HeartFull.png");
        Texture2D heartEmptyTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/HeartEmpty.png");
        Texture2D seedTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/EnergySeed.png");

        GameObject daggerPrefab = CreateDaggerPrefab(daggerSprite);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Cámara
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(-8f, 1f, -10f);

        Camera cam = cameraObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.1f, 0.16f); // Smog industrial
        cameraObj.AddComponent<AudioListener>();

        CameraFollow2D camFollow = cameraObj.AddComponent<CameraFollow2D>();
        cameraObj.AddComponent<AtmosphericParticles>();
        cameraObj.AddComponent<CombatFeedback>();

        // Fondo de Smog con Parallax
        GameObject bgObj = new GameObject("Fondo Smog");
        bgObj.transform.position = new Vector3(20f, 1f, 10f);

        SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -50;
        bgSr.drawMode = SpriteDrawMode.Tiled;
        bgSr.size = new Vector2(180f, 13.5f);
        bgObj.AddComponent<ParallaxBackground>();

        GameObject envParent = new GameObject("--- CIUDAD DE HUMO (ENTORNO) ---");

        // Plataforma Inicial
        CreatePlatform("Plataforma Fábrica 1", new Vector3(-8f, -3.5f, 0f), new Vector3(12f, 1.2f, 1f), Color.white, platformSprite, envParent.transform);

        // Compañero Búho
        CreateCompanion("Búho (Guía de las Alturas)", new Vector3(-5f, -2.1f, 0f), owlSprite,
            "Kiwi: «¿Quién eres tú? El aire aquí quema la garganta...»\n\nBúho: «Soy el vigía de esta ciudad de humo. El Devorador de Humo atrapó al Espíritu del Aire. ¡Usa tu Sobre-Salto [W dos veces] para sortear las chimeneas y derrota a las Nubes Tóxicas con tus dagas!»",
            envParent.transform);

        // Enemigo: Nube Tóxica 1
        CreateToxicCloud("Nube Tóxica 1", new Vector3(2f, -1.0f, 0f), cloudSprite, seedSprite, envParent.transform);

        // Plataformas intermedias de chimenea que requieren el Sobre-Salto
        CreatePlatform("Plataforma Chimenea 1", new Vector3(8f, -1.5f, 0f), new Vector3(6f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateToxicCloud("Nube Tóxica 2", new Vector3(15f, 1.2f, 0f), cloudSprite, seedSprite, envParent.transform);

        CreatePlatform("Plataforma Alta (Sobre-Salto)", new Vector3(17f, 1.0f, 0f), new Vector3(6f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateSeedPickup(new Vector3(17f, 2.5f, 0f), seedSprite, envParent.transform);

        CreatePlatform("Plataforma Chimenea 2", new Vector3(26f, -0.5f, 0f), new Vector3(7f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateToxicCloud("Nube Tóxica 3", new Vector3(26f, 1.5f, 0f), cloudSprite, seedSprite, envParent.transform);

        // Plataforma Final con Portal de retorno al Páramo
        CreatePlatform("Plataforma Mirador", new Vector3(36f, 0.5f, 0f), new Vector3(10f, 1.2f, 1f), Color.white, platformSprite, envParent.transform);
        CreatePortal("Portal Retorno Páramo", new Vector3(38f, 2.0f, 0f), portalSprite, "ParamoToxico", "☠️ Páramo Tóxico (Hub Central)", new Color(0.7f, 0.4f, 0.9f), envParent.transform);

        // Paredes
        CreatePlatform("Pared Izquierda", new Vector3(-14.5f, 3f, 0f), new Vector3(1.2f, 16f, 1f), Color.gray, platformSprite, envParent.transform);
        CreatePlatform("Pared Derecha", new Vector3(42.5f, 5f, 0f), new Vector3(1.2f, 16f, 1f), Color.gray, platformSprite, envParent.transform);

        // Kiwi Protagonista
        GameObject kiwi = CreateKiwi(new Vector3(-9f, -2f, 0f), kiwiSprite, daggerPrefab, heartFullTex, heartEmptyTex, seedTex);
        camFollow.SetTarget(kiwi.transform);

        string path = "Assets/Scenes/CiudadDeHumo.unity";
        EditorSceneManager.SaveScene(scene, path);
        EditorSceneManager.OpenScene(path);

        RegisterAllScenesInBuildSettings();

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "💨 Ciudad de Humo Creada",
                "¡Región 2 (Ciudad de Humo) montada con éxito!\n\n" +
                "• Búho Compañero: Habla con él pulsando [E].\n" +
                "• Nubes Tóxicas: Enemigos espectrales de gas que puedes derrotar con dagas o pisándolos.\n" +
                "• Plataformas de Fábrica: Diseñadas para dominar el Sobre-Salto.\n" +
                "• Portal al Páramo: Al final del recorrido para volver al Hub.\n\n" +
                "¡Pulsa Play (▶) para jugar!",
                "¡A volar!"
            );
        }
    }

    [MenuItem("Kiwi/🔥 Montar Región 3: Desierto de Residuos", priority = 30)]
    public static void SetupDesiertoDeResiduos() => SetupDesiertoDeResiduos(true);
    public static void SetupDesiertoDeResiduos(bool showDialog)
    {
        PixelArtGenerator.GenerateAllSprites();

        Sprite kiwiSprite = LoadSprite("Assets/Sprites/KiwiCat.png");
        Sprite daggerSprite = LoadSprite("Assets/Sprites/KiwiDagger.png");
        Sprite platformSprite = LoadSprite("Assets/Sprites/ScrapPlatform.png");
        Sprite bgSprite = LoadSprite("Assets/Sprites/DesertBackground.png");
        Sprite raccoonSprite = LoadSprite("Assets/Sprites/RaccoonShop.png");
        Sprite slimeSprite = LoadSprite("Assets/Sprites/FireSlime.png");
        Sprite fireBarrierSprite = LoadSprite("Assets/Sprites/FireBarrier.png");
        Sprite turretSprite = LoadSprite("Assets/Sprites/ScrapTurret.png");
        Sprite colmenaSprite = LoadSprite("Assets/Sprites/ColmenaBoss.png");
        Sprite seedSprite = LoadSprite("Assets/Sprites/EnergySeed.png");
        Sprite portalSprite = LoadSprite("Assets/Sprites/PortalVortex.png");

        Texture2D heartFullTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/HeartFull.png");
        Texture2D heartEmptyTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/HeartEmpty.png");
        Texture2D seedTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/EnergySeed.png");

        GameObject daggerPrefab = CreateDaggerPrefab(daggerSprite);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Cámara
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(-8f, 1f, -10f);

        Camera cam = cameraObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.24f, 0.12f, 0.08f); // Tierra ardiente
        cameraObj.AddComponent<AudioListener>();

        CameraFollow2D camFollow = cameraObj.AddComponent<CameraFollow2D>();
        cameraObj.AddComponent<AtmosphericParticles>();
        cameraObj.AddComponent<CombatFeedback>();

        // Fondo del Desierto con Parallax
        GameObject bgObj = new GameObject("Fondo Desierto");
        bgObj.transform.position = new Vector3(20f, 1f, 10f);

        SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -50;
        bgSr.drawMode = SpriteDrawMode.Tiled;
        bgSr.size = new Vector2(180f, 13.5f);
        bgObj.AddComponent<ParallaxBackground>();

        GameObject envParent = new GameObject("--- DESIERTO DE RESIDUOS (ENTORNO) ---");

        // Plataforma Inicial (Campamento del Mapache)
        CreatePlatform("Plataforma Chatarra 1", new Vector3(-8f, -3.5f, 0f), new Vector3(14f, 1.2f, 1f), Color.white, platformSprite, envParent.transform);
        CreateRaccoonShop("Mapache (Chatarrero e Inventor)", new Vector3(-4f, -2.1f, 0f), raccoonSprite, envParent.transform);

        // Enemigo 1: Slime de Fuego (vulnerable a dagas de agua con tecla 1 o Q)
        CreateFireSlime("Slime de Fuego 1", new Vector3(4f, -2.5f, 0f), slimeSprite, seedSprite, envParent.transform);

        // Plataforma Central con Muro de Fuego Puzzle
        CreatePlatform("Plataforma Central", new Vector3(12f, -2.0f, 0f), new Vector3(10f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateFireSlime("Slime de Fuego 2", new Vector3(11f, -1.0f, 0f), slimeSprite, seedSprite, envParent.transform);

        // PUZZLE DE FUEGO: Muro de llamas que bloquea el paso hacia las semillas y atajo
        CreateFireBarrier("Muro de Fuego Tóxico", new Vector3(15f, -0.6f, 0f), fireBarrierSprite, envParent.transform);
        CreateSeedPickup(new Vector3(18f, -1.0f, 0f), seedSprite, envParent.transform);

        // Plataformas Elevadas para probar el Sobre-Salto
        CreatePlatform("Plataforma Alta 1", new Vector3(24f, 0.0f, 0f), new Vector3(6f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateFireSlime("Slime de Fuego 3", new Vector3(24f, 1.2f, 0f), slimeSprite, seedSprite, envParent.transform);

        CreatePlatform("Plataforma Alta 2", new Vector3(32f, 1.8f, 0f), new Vector3(6f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateSeedPickup(new Vector3(32f, 3.2f, 0f), seedSprite, envParent.transform);

        // Plataforma de la Torrecilla Blindada (enemigo táctico con escudo)
        CreatePlatform("Plataforma Torrecilla", new Vector3(41f, 1.0f, 0f), new Vector3(8f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateScrapTurret("Torrecilla Blindada", new Vector3(42f, 2.1f, 0f), turretSprite, seedSprite, envParent.transform);

        // ARENA DEL JEFE: La Colmena Tóxica (Espíritu de Fuego Corrompido)
        CreatePlatform("Arena Colmena", new Vector3(58f, -1.0f, 0f), new Vector3(22f, 1.4f, 1f), Color.white, platformSprite, envParent.transform);
        CreateColmenaBoss("Colmena Tóxica (Jefe de Fuego)", new Vector3(58f, 2.0f, 0f), colmenaSprite, seedSprite, envParent.transform);

        // Plataforma Final con Portal al Páramo
        CreatePlatform("Plataforma Portal", new Vector3(74f, 0.5f, 0f), new Vector3(8f, 1.2f, 1f), Color.white, platformSprite, envParent.transform);
        CreatePortal("Portal Retorno Páramo", new Vector3(75f, 2.0f, 0f), portalSprite, "ParamoToxico", "☠️ Páramo Tóxico (Hub Central)", new Color(0.7f, 0.4f, 0.9f), envParent.transform);

        // Paredes
        CreatePlatform("Pared Izquierda", new Vector3(-15.5f, 3f, 0f), new Vector3(1.2f, 16f, 1f), Color.gray, platformSprite, envParent.transform);
        CreatePlatform("Pared Derecha", new Vector3(79.5f, 5f, 0f), new Vector3(1.2f, 16f, 1f), Color.gray, platformSprite, envParent.transform);

        // Kiwi Protagonista (inicia con agua para apagar fuego)
        GameObject kiwi = CreateKiwi(new Vector3(-10f, -2f, 0f), kiwiSprite, daggerPrefab, heartFullTex, heartEmptyTex, seedTex);
        camFollow.SetTarget(kiwi.transform);

        string path = "Assets/Scenes/DesiertoDeResiduos.unity";
        EditorSceneManager.SaveScene(scene, path);
        EditorSceneManager.OpenScene(path);

        RegisterAllScenesInBuildSettings();

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "🔥 Desierto de Residuos Creado",
                "¡Región 3 (Desierto de Residuos - Fuego) montada con éxito!\n\n" +
                "• Slimes de Fuego: Usa dagas de Agua [1 / Q] para causar daño crítico doble (2x).\n" +
                "• Torrecilla Blindada: ¡Su escudo frontal bloquea ataques! Flanquéala con Torbellino [Shift] o písala.\n" +
                "• Jefe: La Colmena Tóxica: Dispara morteros y fuego al suelo. ¡Apaga el fuego con Agua o salta!\n" +
                "• Mapache Inventor: Habla con él [E] para comprar mejoras.\n\n" +
                "¡Pulsa Play (▶) para jugar!",
                "¡A jugar con fuego y agua!"
            );
        }
    }

    [MenuItem("Kiwi/🌱 Montar Región 4: El Bosque Marchito", priority = 40)]
    public static void SetupBosqueMarchito() => SetupBosqueMarchito(true);
    public static void SetupBosqueMarchito(bool showDialog)
    {
        PixelArtGenerator.GenerateAllSprites();

        Sprite kiwiSprite = LoadSprite("Assets/Sprites/KiwiCat.png");
        Sprite daggerSprite = LoadSprite("Assets/Sprites/KiwiDagger.png");
        Sprite platformSprite = LoadSprite("Assets/Sprites/WoodPlatform.png");
        Sprite bgSprite = LoadSprite("Assets/Sprites/ForestBackground.png");
        Sprite foxSprite = LoadSprite("Assets/Sprites/FoxCompanion.png");
        Sprite specterSprite = LoadSprite("Assets/Sprites/BranchSpecter.png");
        Sprite rootBarrierSprite = LoadSprite("Assets/Sprites/RootBarrier.png");
        Sprite seedSprite = LoadSprite("Assets/Sprites/EnergySeed.png");
        Sprite portalSprite = LoadSprite("Assets/Sprites/PortalVortex.png");

        Texture2D heartFullTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/HeartFull.png");
        Texture2D heartEmptyTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/HeartEmpty.png");
        Texture2D seedTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/EnergySeed.png");

        GameObject daggerPrefab = CreateDaggerPrefab(daggerSprite);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Cámara
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(-8f, 1f, -10f);

        Camera cam = cameraObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.14f, 0.08f); // Bosque crepuscular
        cameraObj.AddComponent<AudioListener>();

        CameraFollow2D camFollow = cameraObj.AddComponent<CameraFollow2D>();
        cameraObj.AddComponent<AtmosphericParticles>();
        cameraObj.AddComponent<CombatFeedback>();

        // Fondo del Bosque con Parallax
        GameObject bgObj = new GameObject("Fondo Bosque");
        bgObj.transform.position = new Vector3(20f, 1f, 10f);

        SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -50;
        bgSr.drawMode = SpriteDrawMode.Tiled;
        bgSr.size = new Vector2(180f, 13.5f);
        bgObj.AddComponent<ParallaxBackground>();

        GameObject envParent = new GameObject("--- BOSQUE MARCHITO (ENTORNO) ---");

        // Plataforma Inicial (Campamento del Zorro)
        CreatePlatform("Plataforma Bosque 1", new Vector3(-8f, -3.5f, 0f), new Vector3(14f, 1.2f, 1f), Color.white, platformSprite, envParent.transform);
        CreateCompanion("Zorro (Guía del Bosque Ancestral)", new Vector3(-4f, -2.1f, 0f), foxSprite,
            "Kiwi: «Este bosque huele a ceniza y madera seca... ¿Qué ha ocurrido?»\n\nZorro: «La degradación del suelo secó las raíces milenarias. ¡Los Espectros de Ramas han bloqueado los caminos! Usa tus dagas de FUEGO [2 / Q] para quemar las barreras de raíces y asestarles daño crítico doble (2x). ¡Salva al Espíritu de la Tierra!»",
            envParent.transform);

        // Enemigo 1: Espectro de Ramas (vulnerable a dagas de fuego [2 / Q])
        CreateBranchSpecter("Espectro de Ramas 1", new Vector3(4f, -2.5f, 0f), specterSprite, seedSprite, envParent.transform);

        // Plataforma Intermedia con Muro de Raíces Secas
        CreatePlatform("Plataforma Raíces", new Vector3(14f, -2.0f, 0f), new Vector3(12f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateBranchSpecter("Espectro de Ramas 2", new Vector3(12f, -1.0f, 0f), specterSprite, seedSprite, envParent.transform);

        // PUZZLE DE TIERRA Y FUEGO: Muro de raíces secas
        CreateRootBarrier("Muro de Raíces Secas", new Vector3(17f, -0.6f, 0f), rootBarrierSprite, envParent.transform);
        CreateSeedPickup(new Vector3(20f, -1.0f, 0f), seedSprite, envParent.transform);

        // Plataformas Elevadas de Madera para dominar el Sobre-Salto
        CreatePlatform("Rama Alta 1", new Vector3(26f, 0.2f, 0f), new Vector3(6f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateBranchSpecter("Espectro de Ramas 3", new Vector3(26f, 1.4f, 0f), specterSprite, seedSprite, envParent.transform);

        CreatePlatform("Rama Alta 2", new Vector3(34f, 2.0f, 0f), new Vector3(6f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateSeedPickup(new Vector3(34f, 3.4f, 0f), seedSprite, envParent.transform);

        // Plataforma Final: Altar del Bosque con Portal de Retorno
        CreatePlatform("Plataforma Altar del Bosque", new Vector3(45f, 0.5f, 0f), new Vector3(12f, 1.2f, 1f), Color.white, platformSprite, envParent.transform);
        CreatePortal("Portal Retorno Páramo", new Vector3(47f, 2.0f, 0f), portalSprite, "ParamoToxico", "☠️ Páramo Tóxico (Hub Central)", new Color(0.3f, 0.9f, 0.35f), envParent.transform);

        // Paredes
        CreatePlatform("Pared Izquierda", new Vector3(-15.5f, 3f, 0f), new Vector3(1.2f, 16f, 1f), Color.gray, platformSprite, envParent.transform);
        CreatePlatform("Pared Derecha", new Vector3(51.5f, 5f, 0f), new Vector3(1.2f, 16f, 1f), Color.gray, platformSprite, envParent.transform);

        // Kiwi Protagonista (con elemento Fuego o ciclo elemental disponible)
        GameObject kiwi = CreateKiwi(new Vector3(-10f, -2f, 0f), kiwiSprite, daggerPrefab, heartFullTex, heartEmptyTex, seedTex);
        camFollow.SetTarget(kiwi.transform);

        string path = "Assets/Scenes/BosqueMarchito.unity";
        EditorSceneManager.SaveScene(scene, path);
        EditorSceneManager.OpenScene(path);

        RegisterAllScenesInBuildSettings();

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "🌱 Bosque Marchito Creado",
                "¡Región 4 (Bosque Marchito - Tierra) montada con éxito!\n\n" +
                "• Zorro Compañero: Habla con él [E] para conocer el destino del bosque.\n" +
                "• Espectros de Ramas: Usa dagas de Fuego [2 / Q] para daño crítico doble (2x).\n" +
                "• Muros de Raíces Secas: ¡Incéndialos con tus dagas de Fuego para abrir los senderos!\n" +
                "• Portal al Páramo: Al final del recorrido para volver al Hub Central.\n\n" +
                "¡Pulsa Play (▶) para jugar!",
                "¡A purificar la tierra!"
            );
        }
    }

    public static void RegisterAllScenesInBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

        string[] scenePaths = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/MarDePlastico.unity",
            "Assets/Scenes/ParamoToxico.unity",
            "Assets/Scenes/CiudadDeHumo.unity",
            "Assets/Scenes/DesiertoDeResiduos.unity",
            "Assets/Scenes/BosqueMarchito.unity"
        };

        foreach (string sp in scenePaths)
        {
            if (File.Exists(sp))
            {
                scenes.Add(new EditorBuildSettingsScene(sp, true));
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"🌍 Escenas registradas en Build Settings: {scenes.Count}");
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null) s = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        return s;
    }

    private static GameObject CreateKiwi(Vector3 pos, Sprite sprite, GameObject daggerPrefab, Texture2D hFull, Texture2D hEmpty, Texture2D sTex)
    {
        GameObject kiwi = new GameObject("Kiwi");
        kiwi.tag = "Player";
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1) kiwi.layer = playerLayer;
        kiwi.transform.position = pos;

        SpriteRenderer sr = kiwi.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;
        kiwi.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

        CapsuleCollider2D col = kiwi.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.7f, 1.25f);
        col.offset = new Vector2(0f, -0.05f);

        KiwiController controller = kiwi.AddComponent<KiwiController>();
        controller.hasDoubleJump = true;

        PlayerHealth health = kiwi.AddComponent<PlayerHealth>();

        KiwiCombat combat = kiwi.AddComponent<KiwiCombat>();
        combat.SetDaggerPrefab(daggerPrefab);

        SimpleHeartHUD hud = kiwi.AddComponent<SimpleHeartHUD>();
        hud.SetupTextures(hFull, hEmpty, sTex);

        // Animador de Sprites HD (Idle, Run, Jump, Attack, Walk)
        kiwi.AddComponent<KiwiSpriteAnimator>();

        // Efectos visuales procedurales
        kiwi.AddComponent<KiwiVisualEffects>();

        // Añadir el mapa mundial al jugador
        kiwi.AddComponent<WorldMapUI>();

        return kiwi;
    }

    private static GameObject CreatePlatform(string name, Vector3 pos, Vector3 scale, Color color, Sprite sprite, Transform parent)
    {
        GameObject plat = new GameObject(name);
        plat.transform.parent = parent;
        plat.transform.position = pos;
        plat.transform.localScale = scale;

        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1) plat.layer = groundLayer;

        SpriteRenderer sr = plat.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(scale.x, scale.y);
        plat.transform.localScale = Vector3.one;

        BoxCollider2D col = plat.AddComponent<BoxCollider2D>();
        col.size = new Vector2(scale.x, scale.y);
        return plat;
    }

    private static GameObject CreateRestTotem(string name, Vector3 pos, Sprite sprite, Transform parent)
    {
        GameObject totem = new GameObject(name);
        totem.transform.parent = parent;
        totem.transform.position = pos;
        totem.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

        SpriteRenderer sr = totem.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        BoxCollider2D col = totem.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.2f, 1.6f);

        totem.AddComponent<RestTotem>();
        return totem;
    }

    private static GameObject CreatePortal(string name, Vector3 pos, Sprite sprite, string destScene, string destDisplay, Color color, Transform parent)
    {
        GameObject portal = new GameObject(name);
        portal.transform.parent = parent;
        portal.transform.position = pos;
        portal.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

        SpriteRenderer sr = portal.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;

        CircleCollider2D col = portal.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.8f;

        RegionPortal rp = portal.AddComponent<RegionPortal>();
        rp.SetDestination(destScene, destDisplay, color);
        return portal;
    }

    private static GameObject CreateCompanion(string name, Vector3 pos, Sprite sprite, string dialog, Transform parent)
    {
        GameObject npc = new GameObject(name);
        npc.transform.parent = parent;
        npc.transform.position = pos;
        npc.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        SpriteRenderer sr = npc.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        CapsuleCollider2D col = npc.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.7f, 0.9f);

        CompanionNPC comp = npc.AddComponent<CompanionNPC>();
        comp.SetDialogue(name, dialog);
        return npc;
    }

    private static GameObject CreateToxicCloud(string name, Vector3 pos, Sprite sprite, Sprite rewardSeed, Transform parent)
    {
        GameObject cloud = new GameObject(name);
        cloud.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) cloud.layer = enemyLayer;
        cloud.transform.parent = parent;
        cloud.transform.position = pos;
        cloud.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        SpriteRenderer sr = cloud.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        CircleCollider2D col = cloud.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.0f;

        Rigidbody2D rb = cloud.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        ToxicCloudAI ai = cloud.AddComponent<ToxicCloudAI>();
        ai.SetRewardSprite(rewardSeed);

        return cloud;
    }

    private static GameObject CreateSeedPickup(Vector3 pos, Sprite sprite, Transform parent)
    {
        GameObject seed = new GameObject("Semilla de Energía");
        seed.transform.parent = parent;
        seed.transform.position = pos;
        seed.transform.localScale = new Vector3(1.4f, 1.4f, 1f);

        SpriteRenderer sr = seed.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        CircleCollider2D col = seed.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        seed.AddComponent<EnergySeedPickup>();
        return seed;
    }

    private static GameObject CreateDaggerPrefab(Sprite sprite)
    {
        GameObject dagger = new GameObject("Dagger");
        SpriteRenderer sr = dagger.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        dagger.transform.localScale = new Vector3(1.3f, 1.3f, 1f);

        BoxCollider2D box = dagger.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(0.8f, 0.35f);

        Rigidbody2D rb = dagger.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.gravityScale = 0f;

        dagger.AddComponent<DaggerProjectile>();

        string prefabPath = "Assets/Prefabs/KiwiDagger.prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(dagger, prefabPath);
        Object.DestroyImmediate(dagger);
        return savedPrefab;
    }

    private static GameObject CreateRaccoonShop(string name, Vector3 pos, Sprite sprite, Transform parent)
    {
        GameObject npc = new GameObject(name);
        npc.transform.parent = parent;
        npc.transform.position = pos;
        npc.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        SpriteRenderer sr = npc.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        CapsuleCollider2D col = npc.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.8f, 0.9f);

        npc.AddComponent<RaccoonShopNPC>();
        return npc;
    }

    private static GameObject CreateFireSlime(string name, Vector3 pos, Sprite sprite, Sprite rewardSeed, Transform parent)
    {
        GameObject slime = new GameObject(name);
        slime.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) slime.layer = enemyLayer;
        slime.transform.parent = parent;
        slime.transform.position = pos;
        slime.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        SpriteRenderer sr = slime.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        CircleCollider2D col = slime.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;

        Rigidbody2D rb = slime.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        FireSlimeAI ai = slime.AddComponent<FireSlimeAI>();
        ai.SetRewardSprite(rewardSeed);

        return slime;
    }

    private static GameObject CreateFireBarrier(string name, Vector3 pos, Sprite sprite, Transform parent)
    {
        GameObject barrier = new GameObject(name);
        barrier.transform.parent = parent;
        barrier.transform.position = pos;
        barrier.transform.localScale = new Vector3(1.2f, 2.5f, 1f);

        SpriteRenderer sr = barrier.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(1.2f, 2.5f);

        BoxCollider2D col = barrier.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.2f, 2.5f);

        barrier.AddComponent<FireBarrier>();
        return barrier;
    }

    private static GameObject CreateScrapTurret(string name, Vector3 pos, Sprite sprite, Sprite rewardSeed, Transform parent)
    {
        GameObject turret = new GameObject(name);
        turret.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) turret.layer = enemyLayer;
        turret.transform.parent = parent;
        turret.transform.position = pos;
        turret.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        SpriteRenderer sr = turret.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        BoxCollider2D col = turret.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);

        ScrapTurretAI ai = turret.AddComponent<ScrapTurretAI>();
        ai.SetRewardSprite(rewardSeed);
        return turret;
    }

    private static GameObject CreateColmenaBoss(string name, Vector3 pos, Sprite sprite, Sprite rewardSeed, Transform parent)
    {
        GameObject boss = new GameObject(name);
        boss.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) boss.layer = enemyLayer;
        boss.transform.parent = parent;
        boss.transform.position = pos;
        boss.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

        SpriteRenderer sr = boss.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        CircleCollider2D col = boss.AddComponent<CircleCollider2D>();
        col.radius = 0.9f;

        Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.gravityScale = 0f;

        ToxicHiveBossAI ai = boss.AddComponent<ToxicHiveBossAI>();
        ai.SetupArena(pos.x - 7f, pos.x + 7f, sprite, rewardSeed);
        return boss;
    }

    private static GameObject CreateBranchSpecter(string name, Vector3 pos, Sprite sprite, Sprite rewardSeed, Transform parent)
    {
        GameObject specter = new GameObject(name);
        specter.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) specter.layer = enemyLayer;
        specter.transform.parent = parent;
        specter.transform.position = pos;
        specter.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        SpriteRenderer sr = specter.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        CapsuleCollider2D col = specter.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(1.2f, 2.2f);

        Rigidbody2D rb = specter.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        BranchSpecterAI ai = specter.AddComponent<BranchSpecterAI>();
        ai.SetRewardSprite(rewardSeed);
        return specter;
    }

    private static GameObject CreateRootBarrier(string name, Vector3 pos, Sprite sprite, Transform parent)
    {
        GameObject barrier = new GameObject(name);
        barrier.transform.parent = parent;
        barrier.transform.position = pos;
        barrier.transform.localScale = new Vector3(1.2f, 2.5f, 1f);

        SpriteRenderer sr = barrier.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(1.2f, 2.5f);

        BoxCollider2D col = barrier.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.2f, 2.5f);

        barrier.AddComponent<WitheredRootBarrier>();
        return barrier;
    }
}
#endif
