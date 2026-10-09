#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Platformer2D;

/// <summary>
/// Herramienta del Editor de Unity para construir automáticamente la región 1 del juego de Kiwi:
/// "El Mar de Plástico" (MVP Completo con Kiwi, Sobre-Salto, Dagas, Golems, Moscas Voladoras,
/// Jefe Regional La Ballena de Plástico y Altar de Purificación).
/// </summary>
public static class AutoSetupScene
{
    [MenuItem("Kiwi/🌊 Montar Región 1: Mar de Plástico (MVP Completo)")]
    public static void SetupScene() => SetupScene(true);

    public static void SetupScene(bool showDialog)
    {
        // 1. Cargar sprites pixel art del juego
        PixelArtGenerator.GenerateAllSprites();
        Sprite kiwiSprite = LoadSprite("Assets/Sprites/KiwiCat.png");
        Sprite daggerSprite = LoadSprite("Assets/Sprites/KiwiDagger.png");
        Sprite golemSprite = LoadSprite("Assets/Sprites/PlasticGolem.png");
        Sprite platformSprite = LoadSprite("Assets/Sprites/TrashPlatform.png");
        Sprite toxicSprite = LoadSprite("Assets/Sprites/ToxicWater.png");
        Sprite seedSprite = LoadSprite("Assets/Sprites/EnergySeed.png");
        Sprite oceanBgSprite = LoadSprite("Assets/Sprites/OceanBackground.png");
        Sprite penguinSprite = LoadSprite("Assets/Sprites/PenguinCompanion.png");
        Sprite flySprite = LoadSprite("Assets/Sprites/TrashFly.png");
        Sprite whaleBossSprite = LoadSprite("Assets/Sprites/PlasticWhaleBoss.png");
        Sprite portalSprite = LoadSprite("Assets/Sprites/PortalVortex.png");

        Texture2D heartFullTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/HeartFull.png");
        Texture2D heartEmptyTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/HeartEmpty.png");
        Texture2D seedTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/EnergySeed.png");

        // 2. Crear el Prefab de la Daga Arrojadiza con Rigidbody2D cinemático
        GameObject daggerPrefab = CreateDaggerPrefab(daggerSprite);

        // 3. Crear una nueva escena limpia
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 4. Configurar Cámara 2D
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(-8f, 1f, -10f);

        Camera cam = cameraObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.11f, 0.16f); // Océano oscuro contaminado
        cameraObj.AddComponent<AudioListener>();

        CameraFollow2D camFollow = cameraObj.AddComponent<CameraFollow2D>();
        cameraObj.AddComponent<AtmosphericParticles>();
        cameraObj.AddComponent<CombatFeedback>();

        // 5. FONDO MARINO CON PARALLAX
        GameObject bgObj = new GameObject("Fondo Marino");
        bgObj.transform.position = new Vector3(25f, 1f, 10f);

        SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = oceanBgSprite;
        bgSr.sortingOrder = -50;
        bgSr.drawMode = SpriteDrawMode.Tiled;
        bgSr.size = new Vector2(180f, 13.5f);
        bgObj.transform.localScale = Vector3.one;

        bgObj.AddComponent<ParallaxBackground>();

        // 6. Contenedor de Entorno
        GameObject envParent = new GameObject("--- MAR DE PLÁSTICO (ENTORNO) ---");

        // Suelo Inicial Seguro (Campamento con el Pingüino)
        CreatePlatform("Plataforma Inicio", new Vector3(-8f, -3.5f, 0f), new Vector3(14f, 1.2f, 1f), Color.white, platformSprite, envParent.transform);
        CreateCompanion("Pingüino (Guía Acuático)", new Vector3(-5f, -2.1f, 0f), penguinSprite, envParent.transform);

        // Foso de Residuos Tóxicos 1
        CreateHazard("Foso Tóxico 1", new Vector3(3f, -4.8f, 0f), new Vector3(8f, 1.5f, 1f), toxicSprite, envParent.transform);
        // Enemigo volador: Mosca de Basura 1 (se puede eliminar con dagas o pisándola)
        CreateTrashFly("Mosca de Basura 1", new Vector3(3f, -0.6f, 0f), flySprite, seedSprite, envParent.transform);

        // Plataforma Flotante 1 con Golem vigilante
        CreatePlatform("Plataforma Central", new Vector3(12f, -2.0f, 0f), new Vector3(10f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateGolem("Golem de Plástico 1", new Vector3(12f, -0.8f, 0f), 7.8f, 16.2f, golemSprite, seedSprite, envParent.transform);

        // Foso de Residuos Tóxicos 2
        CreateHazard("Foso Tóxico 2", new Vector3(21f, -4.8f, 0f), new Vector3(8f, 1.5f, 1f), toxicSprite, envParent.transform);
        // Enemigo volador: Mosca de Basura 2
        CreateTrashFly("Mosca de Basura 2", new Vector3(21f, 0.4f, 0f), flySprite, seedSprite, envParent.transform);

        // Plataformas Elevadas para probar el SOBRE-SALTO (doble salto más alto)
        CreatePlatform("Plataforma Salto 1", new Vector3(27f, -0.5f, 0f), new Vector3(6f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreatePlatform("Plataforma Alta (Sobre-Salto Requerido)", new Vector3(34f, 2.2f, 0f), new Vector3(6f, 1f, 1f), Color.white, platformSprite, envParent.transform);
        CreateTrashFly("Mosca de Basura 3", new Vector3(34f, 3.8f, 0f), flySprite, seedSprite, envParent.transform);

        // ARENA DEL JEFE REGIONAL: LA BALLENA DE PLÁSTICO (Espíritu de Agua Corrompido)
        CreatePlatform("Arena de la Ballena", new Vector3(45f, -1.5f, 0f), new Vector3(14f, 1.2f, 1f), Color.white, platformSprite, envParent.transform);
        CreateWhaleBoss("La Ballena de Plástico (Jefe)", new Vector3(46f, 1.8f, 0f), 39f, 51f, whaleBossSprite, seedSprite, envParent.transform);

        // Zona Final: El Altar de la Última Semilla y Portal al Páramo Tóxico
        CreatePlatform("Plataforma del Santuario", new Vector3(60f, 0.0f, 0f), new Vector3(12f, 1.2f, 1f), Color.white, platformSprite, envParent.transform);
        CreateAltar("Altar de Purificación", new Vector3(62f, 1.3f, 0f), seedSprite, envParent.transform);
        CreatePortal("Portal al Páramo Tóxico", new Vector3(64.5f, 1.3f, 0f), portalSprite, "ParamoToxico", "☠️ Páramo Tóxico (Hub Central)", new Color(0.7f, 0.4f, 0.9f), envParent.transform);

        // Paredes delimitadoras del nivel
        CreatePlatform("Pared Izquierda", new Vector3(-15.5f, 3f, 0f), new Vector3(1.2f, 16f, 1f), Color.gray, platformSprite, envParent.transform);
        CreatePlatform("Pared Derecha", new Vector3(67.5f, 5f, 0f), new Vector3(1.2f, 16f, 1f), Color.gray, platformSprite, envParent.transform);

        // Semillas de Energía coleccionables distribuidas por el mapa
        CreateSeedPickup(new Vector3(-4f, -1.8f, 0f), seedSprite, envParent.transform);
        CreateSeedPickup(new Vector3(3f, 1.4f, 0f), seedSprite, envParent.transform);
        CreateSeedPickup(new Vector3(15f, -0.3f, 0f), seedSprite, envParent.transform);
        CreateSeedPickup(new Vector3(21f, 2.5f, 0f), seedSprite, envParent.transform);
        CreateSeedPickup(new Vector3(34f, 4.0f, 0f), seedSprite, envParent.transform);

        // 7. CREAR AL PROTAGONISTA: KIWI
        GameObject kiwi = new GameObject("Kiwi");
        kiwi.tag = "Player";
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1) kiwi.layer = playerLayer;
        kiwi.transform.position = new Vector3(-10f, -2f, 0f);

        // Sprite Pixel Art de Kiwi
        SpriteRenderer sr = kiwi.AddComponent<SpriteRenderer>();
        sr.sprite = kiwiSprite;
        sr.color = Color.white;
        kiwi.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

        // Colisionador ajustado al Sprite HD
        CapsuleCollider2D col = kiwi.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.7f, 1.25f);
        col.offset = new Vector2(0f, -0.05f);

        // Rigidbody2D y Controlador
        KiwiController controller = kiwi.AddComponent<KiwiController>();
        controller.hasDoubleJump = true; // ¡Sobre-Salto activo desde el inicio!

        int groundLayerIdx = LayerMask.NameToLayer("Ground");
        if (groundLayerIdx != -1)
        {
            SerializedObject so = new SerializedObject(controller);
            SerializedProperty prop = so.FindProperty("groundLayer");
            if (prop != null)
            {
                prop.intValue = 1 << groundLayerIdx;
                so.ApplyModifiedProperties();
            }
        }

        // Salud de 4 corazones
        PlayerHealth health = kiwi.AddComponent<PlayerHealth>();

        // Combate (lanzamiento de dagas)
        KiwiCombat combat = kiwi.AddComponent<KiwiCombat>();
        combat.SetDaggerPrefab(daggerPrefab);

        // HUD de Corazones, Energía y Semillas
        SimpleHeartHUD hud = kiwi.AddComponent<SimpleHeartHUD>();
        hud.SetupTextures(heartFullTex, heartEmptyTex, seedTex);

        // Conectar la cámara a Kiwi
        camFollow.SetTarget(kiwi.transform);

        // Animador de Sprites HD (Idle, Run, Jump, Attack, Walk)
        kiwi.AddComponent<KiwiSpriteAnimator>();

        // Efectos visuales procedurales (Squash, Stretch, Run Tilt, Ghost Dash)
        kiwi.AddComponent<KiwiVisualEffects>();

        // Mapa mundial interactivo con la tecla [M]
        kiwi.AddComponent<WorldMapUI>();

        // 8. Guardar la escena en Assets/Scenes/MarDePlastico.unity
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
        string scenePath = "Assets/Scenes/MarDePlastico.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorSceneManager.OpenScene(scenePath);

        WorldBuilder.RegisterAllScenesInBuildSettings();

        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.in2DMode = true;
            SceneView.lastActiveSceneView.FrameSelected();
        }

        Selection.activeGameObject = kiwi;

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "🌊 Región 1: Mar de Plástico Actualizada",
                "¡Nivel enriquecido con las novedades que pediste!\n\n" +
                "• Salto fijo y ¡SOBRE-SALTO! (pulsa [W] en el suelo para saltar y otra vez en el aire para impulsarte más alto).\n" +
                "• Moscas de Basura derrotables: ahora puedes matarlas lanzando tus dagas [J] o pisándolas desde arriba.\n" +
                "• Jefe Regional: ¡La Ballena de Plástico! con 2 fases (disparos y embestidas furiosas) y purificación.\n" +
                "• Diálogo narrativo con tu compañero Pingüino [E].\n" +
                "• Efectos de sonido integrados para saltos, sobre-salto, dagas y victoria.\n\n" +
                "¡Pulsa el botón Play (▶) arriba para jugar!",
                "¡A jugar!"
            );
        }
    }

    private static Sprite LoadSprite(string path)
    {
        EnsureSpriteImporter(path);
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null) s = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        return s;
    }

    private static void EnsureSpriteImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
        }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    private static GameObject CreateDaggerPrefab(Sprite sprite)
    {
        GameObject dagger = new GameObject("Dagger");
        SpriteRenderer sr = dagger.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;
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

    private static GameObject CreateHazard(string name, Vector3 pos, Vector3 scale, Sprite sprite, Transform parent)
    {
        GameObject haz = new GameObject(name);
        haz.tag = "Hazard";
        haz.transform.parent = parent;
        haz.transform.position = pos;

        int hazardLayer = LayerMask.NameToLayer("Hazard");
        if (hazardLayer != -1) haz.layer = hazardLayer;

        SpriteRenderer sr = haz.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.8f, 1f, 0.8f, 0.9f);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(scale.x, scale.y);

        BoxCollider2D col = haz.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(scale.x, scale.y);

        haz.AddComponent<HazardTrap>();
        return haz;
    }

    private static GameObject CreateGolem(string name, Vector3 pos, float minX, float maxX, Sprite sprite, Sprite rewardSeed, Transform parent)
    {
        GameObject golem = new GameObject(name);
        golem.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) golem.layer = enemyLayer;
        golem.transform.parent = parent;
        golem.transform.position = pos;
        golem.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        SpriteRenderer sr = golem.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;

        CapsuleCollider2D col = golem.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(1.5f, 2.2f);

        Rigidbody2D rb = golem.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        PlasticGolemAI ai = golem.AddComponent<PlasticGolemAI>();
        ai.SetPatrolLimits(minX, maxX);
        ai.SetRewardSprite(rewardSeed);

        return golem;
    }

    private static GameObject CreateCompanion(string name, Vector3 pos, Sprite sprite, Transform parent)
    {
        GameObject npc = new GameObject(name);
        npc.transform.parent = parent;
        npc.transform.position = pos;
        npc.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        SpriteRenderer sr = npc.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;

        CapsuleCollider2D col = npc.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.7f, 0.9f);

        npc.AddComponent<CompanionNPC>();
        return npc;
    }

    private static GameObject CreateTrashFly(string name, Vector3 pos, Sprite sprite, Sprite rewardSeed, Transform parent)
    {
        GameObject fly = new GameObject(name);
        fly.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) fly.layer = enemyLayer;
        fly.transform.parent = parent;
        fly.transform.position = pos;
        fly.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        SpriteRenderer sr = fly.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;

        CircleCollider2D col = fly.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Rigidbody2D rb = fly.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.gravityScale = 0f;

        TrashFlyAI ai = fly.AddComponent<TrashFlyAI>();
        ai.SetRewardSprite(rewardSeed);

        return fly;
    }

    private static GameObject CreateWhaleBoss(string name, Vector3 pos, float minX, float maxX, Sprite sprite, Sprite rewardSeed, Transform parent)
    {
        GameObject boss = new GameObject(name);
        boss.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) boss.layer = enemyLayer;
        boss.transform.parent = parent;
        boss.transform.position = pos;
        boss.transform.localScale = new Vector3(2.4f, 2.4f, 1f); // Imponente presencia de jefe

        SpriteRenderer sr = boss.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;

        CapsuleCollider2D col = boss.AddComponent<CapsuleCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2.4f, 1.4f);
        col.direction = CapsuleDirection2D.Horizontal;

        Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.gravityScale = 0f;

        PlasticWhaleBossAI ai = boss.AddComponent<PlasticWhaleBossAI>();
        ai.Setup(null, rewardSeed);

        return boss;
    }

    private static GameObject CreateSeedPickup(Vector3 pos, Sprite sprite, Transform parent)
    {
        GameObject seed = new GameObject("Semilla de Energía");
        seed.transform.parent = parent;
        seed.transform.position = pos;
        seed.transform.localScale = new Vector3(1.4f, 1.4f, 1f);

        SpriteRenderer sr = seed.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;

        CircleCollider2D col = seed.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        seed.AddComponent<EnergySeedPickup>();
        return seed;
    }

    private static GameObject CreateAltar(string name, Vector3 pos, Sprite seedSprite, Transform parent)
    {
        GameObject altar = new GameObject(name);
        altar.transform.parent = parent;
        altar.transform.position = pos;
        altar.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

        SpriteRenderer sr = altar.AddComponent<SpriteRenderer>();
        sr.sprite = seedSprite;
        sr.color = new Color(0.2f, 0.8f, 1f);

        BoxCollider2D col = altar.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1.5f);

        altar.AddComponent<PurificationAltar>();
        return altar;
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
}
#endif
