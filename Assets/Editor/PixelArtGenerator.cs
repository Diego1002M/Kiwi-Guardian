#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generador de sprites Pixel Art de Alta Definición para Kiwi: El Guardián de la Última Semilla.
/// Produce gráficos estilizados estilo Celeste / Hollow Knight con sombreado, paletas profesionales,
/// iluminación de bordes y fondos panorámicos de 256x128 con luna, olas y siluetas.
/// </summary>
public static class PixelArtGenerator
{
    [MenuItem("Kiwi/🎨 Generar Sprites del Mundo Completo (Pixel Art HD)")]
    public static void GenerateAllSprites()
    {
        // 1. Personajes y Enemigos de la Región 1 (Mar de Plástico)
        GenerateKiwiCat();
        GeneratePlasticWhaleBoss();
        GeneratePlasticGolem();
        GenerateTrashFly();
        GeneratePenguinCompanion();
        GenerateOceanBackground();
        GenerateTrashPlatform();
        GenerateToxicWater();
        GenerateKiwiDagger();
        GenerateEnergySeed();
        GenerateHeartFull();
        GenerateHeartEmpty();

        // 2. Hub Central y Portales
        GenerateRestTotem();
        GeneratePortalVortex();

        // 3. Región 2: Ciudad de Humo (Aire)
        GenerateOwlCompanion();
        GenerateToxicCloud();
        GenerateFactoryPlatform();
        GenerateSmogBackground();

        // 4. Región 3: Desierto de Residuos (Fuego)
        GenerateFireSlime();
        GenerateFireBarrier();
        GenerateRaccoonShop();
        GenerateDesertBackground();
        GenerateScrapPlatform();
        GenerateColmenaBoss();
        GenerateScrapTurret();

        // 5. Región 4: Bosque Marchito (Tierra)
        GenerateFoxCompanion();
        GenerateBranchSpecter();
        GenerateForestBackground();
        GenerateWoodPlatform();
        GenerateRootBarrier();

        // 6. Pantalla de Título y Prólogo Narrativo
        GenerateTitleLogo();
        GenerateGiantSeed();
        GeneratePrologueSlides();

        // 7. Extraer y aplicar el Arte Oficial de las Capturas del Usuario (HD)
        if (File.Exists("Assets/ArtSource/KiwiSheetSource.png"))
        {
            UserArtAssetExtractor.ExtractAndApplyAllArt(false);
        }

        AssetDatabase.Refresh();
        Debug.Log("🎨 ¡Todos los Sprites HD del Mundo de Kiwi han sido generados con éxito!");
    }

    private static void SaveSprite(Texture2D tex, string path, int ppu = 16, bool clampV = false)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.spritePixelsPerUnit = ppu;
            importer.wrapModeU = TextureWrapMode.Repeat;
            importer.wrapModeV = clampV ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static Texture2D CreateClearTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color transparent = new Color(0, 0, 0, 0);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, transparent);
        return tex;
    }

    // =========================================================================
    // 🐱 1. KIWI EL GATO GUARDIÁN (32x32 HD) - Diseño Fiel al Concepto
    // =========================================================================
    private static void GenerateKiwiCat()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color outline = new Color(0.18f, 0.08f, 0.04f);      // Contorno marrón oscuro
        Color furOrange = new Color(0.92f, 0.52f, 0.16f);    // Pelaje naranja cálido (atigrado)
        Color furShadow = new Color(0.74f, 0.36f, 0.1f);     // Sombra pelaje
        Color furLight = new Color(1.0f, 0.68f, 0.32f);      // Brillo pelaje dorado
        Color furStripe = new Color(0.58f, 0.24f, 0.06f);    // Rayas atigradas
        Color creamMuzzle = new Color(0.98f, 0.94f, 0.86f);  // Hocico y pecho crema
        Color earPink = new Color(1.0f, 0.52f, 0.64f);       // Interior orejitas
        Color eyeGreen = new Color(0.2f, 0.82f, 0.38f);      // Ojos esmeralda vivos (como la referencia)
        Color eyePupil = new Color(0.06f, 0.18f, 0.08f);     // Pupila rasgada
        Color eyeShine = Color.white;
        Color nose = new Color(1f, 0.45f, 0.55f);            // Nariz rosada

        // Capucha y chaqueta verde azulada (Teal / Verde petróleo según la referencia)
        Color hoodieTeal = new Color(0.12f, 0.45f, 0.48f);   // Chaqueta verde azulada
        Color hoodieShadow = new Color(0.06f, 0.28f, 0.3f);  // Sombra capucha
        Color hoodieLight = new Color(0.22f, 0.64f, 0.66f);  // Brillo capucha
        Color hoodieZipper = new Color(0.92f, 0.82f, 0.4f);  // Cremallera dorada

        // Mochila de explorador marrón y Gema de la Última Semilla
        Color packLeather = new Color(0.48f, 0.28f, 0.15f);  // Cuero marrón
        Color packDark = new Color(0.32f, 0.18f, 0.09f);
        Color crystalCyan = new Color(0.2f, 0.85f, 1.0f);    // Cristal azul brillante de la semilla
        Color crystalCore = Color.white;
        Color seedSprout = new Color(0.35f, 0.95f, 0.4f);    // Brote verde sagrado

        // 1. Cola curva atigrada naranja con punta crema (izquierda)
        int[,] tail = { {5, 7}, {4, 8}, {4, 9}, {4, 10}, {5, 11}, {6, 12}, {7, 12} };
        for (int i = 0; i < tail.GetLength(0); i++)
        {
            int tx = tail[i, 0], ty = tail[i, 1];
            Color c = (i > 4) ? creamMuzzle : ((i % 2 == 0) ? furStripe : furOrange);
            tex.SetPixel(tx, ty, c);
        }

        // 2. Mochila de Explorador en la espalda (izquierda)
        for (int x = 7; x <= 11; x++)
        {
            for (int y = 8; y <= 16; y++)
            {
                if (x == 7 || y == 8 || y == 16) tex.SetPixel(x, y, packDark);
                else tex.SetPixel(x, y, packLeather);
            }
        }
        // Gema / Cristal de la Última Semilla montado en el centro de la mochila
        tex.SetPixel(8, 11, crystalCyan); tex.SetPixel(9, 11, crystalCyan);
        tex.SetPixel(8, 12, crystalCore); tex.SetPixel(9, 12, crystalCyan);
        tex.SetPixel(8, 13, seedSprout); // Brote

        // 3. Torso con Chaqueta Teal / Sudadera con capucha
        for (int x = 11; x <= 22; x++)
        {
            for (int y = 5; y <= 15; y++)
            {
                if (x == 11 || x == 22 || y == 5) tex.SetPixel(x, y, outline);
                else if (x >= 15 && x <= 17 && y >= 6 && y <= 13) tex.SetPixel(x, y, hoodieZipper); // Cremallera
                else if (x > 17) tex.SetPixel(x, y, hoodieLight);
                else tex.SetPixel(x, y, (y < 8) ? hoodieShadow : hoodieTeal);
            }
        }

        // Cinturón de cuero con hebilla metálica
        for (int x = 12; x <= 21; x++)
        {
            tex.SetPixel(x, 7, (x == 16 || x == 17) ? Color.yellow : packDark);
        }

        // Patitas con mitones de cuero marrón
        tex.SetPixel(13, 3, outline); tex.SetPixel(14, 3, packDark); tex.SetPixel(15, 3, packLeather);
        tex.SetPixel(13, 4, packDark); tex.SetPixel(14, 4, packLeather); tex.SetPixel(15, 4, packLeather);
        tex.SetPixel(18, 3, outline); tex.SetPixel(19, 3, packDark); tex.SetPixel(20, 3, packLeather);
        tex.SetPixel(18, 4, packDark); tex.SetPixel(19, 4, packLeather); tex.SetPixel(20, 4, packLeather);

        // 4. Cuello y Capucha verde azulada doblada
        for (int x = 10; x <= 23; x++)
        {
            for (int y = 16; y <= 18; y++)
            {
                tex.SetPixel(x, y, (y == 16) ? hoodieShadow : hoodieTeal);
            }
        }
        tex.SetPixel(9, 16, hoodieShadow); tex.SetPixel(10, 15, hoodieTeal);

        // 5. Cabeza del Gato Naranja
        for (int x = 11; x <= 23; x++)
        {
            for (int y = 19; y <= 27; y++)
            {
                if (x == 11 || x == 23 || y == 27) tex.SetPixel(x, y, outline);
                else if (x >= 19) tex.SetPixel(x, y, furLight);
                else tex.SetPixel(x, y, (y >= 25 && (x == 15 || x == 19)) ? furStripe : furOrange);
            }
        }
        // Rayas en la frente
        tex.SetPixel(17, 26, furStripe); tex.SetPixel(17, 25, furStripe);

        // Orejitas puntiagudas felinas
        for (int y = 28; y <= 31; y++)
        {
            int wRow = 32 - y;
            for (int ox = 0; ox < wRow; ox++)
            {
                tex.SetPixel(12 + ox, y, (ox == 0) ? outline : (ox == 1 ? earPink : furOrange));
                tex.SetPixel(22 - ox, y, (ox == 0) ? outline : (ox == 1 ? earPink : furLight));
            }
        }

        // Ojos Verdes Esmeralda (según referencia del usuario)
        // Ojo Izquierdo
        tex.SetPixel(14, 22, eyeGreen); tex.SetPixel(15, 22, eyeGreen);
        tex.SetPixel(14, 23, eyeGreen); tex.SetPixel(15, 23, eyeGreen);
        tex.SetPixel(15, 22, eyePupil); tex.SetPixel(15, 23, eyePupil);
        tex.SetPixel(14, 23, eyeShine);

        // Ojo Derecho
        tex.SetPixel(19, 22, eyeGreen); tex.SetPixel(20, 22, eyeGreen);
        tex.SetPixel(19, 23, eyeGreen); tex.SetPixel(20, 23, eyeGreen);
        tex.SetPixel(19, 22, eyePupil); tex.SetPixel(19, 23, eyePupil);
        tex.SetPixel(20, 23, eyeShine);

        // Hocico claro y naricita rosada
        tex.SetPixel(17, 21, nose);
        tex.SetPixel(16, 20, creamMuzzle); tex.SetPixel(17, 20, creamMuzzle); tex.SetPixel(18, 20, creamMuzzle);
        tex.SetPixel(16, 19, creamMuzzle); tex.SetPixel(18, 19, creamMuzzle);

        // Bigotitos
        tex.SetPixel(12, 21, Color.white); tex.SetPixel(13, 21, Color.white);
        tex.SetPixel(21, 21, Color.white); tex.SetPixel(22, 21, Color.white);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/KiwiCat.png");
    }

    // =========================================================================
    // 🐋 2. LA BALLENA DE PLÁSTICO (LEVIATÁN 96x48 HD)
    // =========================================================================
    private static void GeneratePlasticWhaleBoss()
    {
        int w = 96, h = 48;
        Texture2D tex = CreateClearTexture(w, h);

        Color plateDark = new Color(0.08f, 0.14f, 0.22f);
        Color plateMid = new Color(0.14f, 0.24f, 0.36f);
        Color plateLight = new Color(0.25f, 0.42f, 0.58f);
        Color underbelly = new Color(0.38f, 0.45f, 0.52f);
        Color seamCyan = new Color(0.1f, 0.85f, 0.95f);
        Color corePink = new Color(1f, 0.2f, 0.6f);
        Color coreHot = new Color(1f, 0.9f, 0.98f);
        Color eyeRed = new Color(1f, 0.15f, 0.15f);
        Color turbineCyan = new Color(0.3f, 0.95f, 1f);

        // Cuerpo elíptico hidrodinámico
        for (int x = 8; x <= 84; x++)
        {
            for (int y = 8; y <= 38; y++)
            {
                float dx = (x - 45f) / 38f;
                float dy = (y - 23f) / 14f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    // Dividir placas superior e inferior
                    if (y < 18) tex.SetPixel(x, y, ((x + y) % 8 == 0) ? plateDark : underbelly);
                    else if (y > 32) tex.SetPixel(x, y, plateLight);
                    else tex.SetPixel(x, y, plateMid);

                    // Líneas de remaches y junturas
                    if (x % 16 == 0 || y == 24) tex.SetPixel(x, y, seamCyan);
                }
            }
        }

        // Cabeza y mandíbula mecánica (izquierda)
        for (int y = 14; y <= 28; y++)
        {
            for (int x = 4; x <= 14; x++)
            {
                if (y % 3 == 0) tex.SetPixel(x, y, plateLight); // Rejilla de filtración de plástico
            }
        }
        // Ojo óptico amenazante
        for (int ox = 18; ox <= 21; ox++)
            for (int oy = 26; oy <= 29; oy++)
                tex.SetPixel(ox, oy, eyeRed);
        tex.SetPixel(19, 28, Color.white);

        // Aleta dorsal metálica
        for (int x = 44; x <= 56; x++)
        {
            int top = 38 + (int)((6 - Mathf.Abs(x - 50)) * 1.4f);
            for (int y = 38; y <= top; y++) tex.SetPixel(x, y, (x > 50) ? plateLight : plateMid);
        }

        // Aleta pectoral lateral
        for (int x = 30; x <= 46; x++)
        {
            int bot = 12 - (int)((8 - Mathf.Abs(x - 38)) * 0.8f);
            for (int y = bot; y <= 14; y++) tex.SetPixel(x, y, plateDark);
        }

        // Cola de doble aleta (derecha)
        for (int y = 12; y <= 34; y++)
        {
            int flukeW = (int)(Mathf.Abs(y - 23) * 0.9f);
            for (int x = 82; x <= 82 + flukeW; x++)
            {
                if (x < w) tex.SetPixel(x, y, (y > 23) ? plateLight : plateDark);
            }
        }
        // Turbina trasera
        for (int y = 20; y <= 26; y++)
        {
            tex.SetPixel(84, y, turbineCyan);
            tex.SetPixel(85, y, Color.white);
        }

        // NÚCLEO SAGRADO CORROMPIDO (Espíritu del Agua atrapado)
        for (int x = 42; x <= 52; x++)
        {
            for (int y = 20; y <= 28; y++)
            {
                float dx = (x - 47f) / 5f;
                float dy = (y - 24f) / 4f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    tex.SetPixel(x, y, (dx * dx + dy * dy < 0.35f) ? coreHot : corePink);
                }
            }
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/PlasticWhaleBoss.png");
    }

    // =========================================================================
    // 🤖 3. GÓLEM DE PLÁSTICO Y RESIDUOS (36x40 HD) - Fiel a la Referencia
    // =========================================================================
    private static void GeneratePlasticGolem()
    {
        int w = 36, h = 40;
        Texture2D tex = CreateClearTexture(w, h);

        Color outline = new Color(0.12f, 0.15f, 0.2f);
        Color bodyTrash = new Color(0.55f, 0.62f, 0.68f);   // Cuerpo de residuos compactados
        Color bodyDark = new Color(0.35f, 0.42f, 0.48f);
        Color bottleBlue = new Color(0.2f, 0.65f, 0.95f);   // Botellas de plástico azul
        Color bottleGreen = new Color(0.25f, 0.85f, 0.45f); // Botellas verdes
        Color bottleCap = Color.white;
        Color netRope = new Color(0.25f, 0.35f, 0.45f);     // Red de pesca gris oscura
        Color eyeRed = new Color(1.0f, 0.15f, 0.15f);       // Ojos rojos brillantes
        Color toothWhite = new Color(0.95f, 0.95f, 0.92f);  // Dientes afilados de plástico
        Color rustBag = new Color(0.72f, 0.45f, 0.22f);     // Bolsas y cartón descompuesto

        // Piernas encorvadas y pesadas
        for (int y = 2; y <= 9; y++)
        {
            for (int x = 6; x <= 13; x++) tex.SetPixel(x, y, (x == 6 || y == 2) ? outline : bodyDark);
            for (int x = 20; x <= 27; x++) tex.SetPixel(x, y, (x == 27 || y == 2) ? outline : bodyTrash);
        }

        // Joroba y cuerpo masivo de plástico y bolsas
        for (int x = 5; x <= 30; x++)
        {
            for (int y = 10; y <= 28; y++)
            {
                if (x == 5 || x == 30 || y == 10) tex.SetPixel(x, y, outline);
                else
                {
                    // Textura de basura compactada con manchas de bolsas
                    bool isBag = (x + y * 2) % 7 == 0;
                    tex.SetPixel(x, y, isBag ? rustBag : ((x > 18) ? bodyTrash : bodyDark));
                }
            }
        }

        // Red de pesca tejida sobre el hombro (patrón de red como la imagen 1)
        for (int x = 8; x <= 22; x += 3)
        {
            for (int y = 12; y <= 24; y += 3)
            {
                tex.SetPixel(x, y, netRope);
                tex.SetPixel(x + 1, y + 1, netRope);
            }
        }

        // Botellas de plástico clavadas en la joroba de la espalda (izquierda)
        int[,] bottles = { {4, 24}, {5, 27}, {7, 30}, {10, 32}, {6, 21} };
        for (int i = 0; i < bottles.GetLength(0); i++)
        {
            int bx = bottles[i, 0], by = bottles[i, 1];
            Color bc = (i % 2 == 0) ? bottleBlue : bottleGreen;
            tex.SetPixel(bx, by, bc);
            tex.SetPixel(bx + 1, by, bc);
            tex.SetPixel(bx, by + 1, bottleCap);
        }

        // Cabeza feroz encorvada
        for (int x = 14; x <= 32; x++)
        {
            for (int y = 25; y <= 37; y++)
            {
                if (x == 32 || y == 37) tex.SetPixel(x, y, outline);
                else tex.SetPixel(x, y, (x > 22) ? bodyTrash : bodyDark);
            }
        }

        // Ojos Rojos Amenazantes (como la referencia 1)
        // Ojo 1
        tex.SetPixel(22, 32, eyeRed); tex.SetPixel(23, 32, eyeRed);
        tex.SetPixel(22, 33, eyeRed); tex.SetPixel(23, 33, Color.white);
        // Ojo 2
        tex.SetPixel(28, 32, eyeRed); tex.SetPixel(29, 32, eyeRed);
        tex.SetPixel(28, 33, eyeRed); tex.SetPixel(29, 33, Color.white);

        // Fauces con dientes afilados de plástico quebrado
        for (int x = 18; x <= 30; x++) tex.SetPixel(x, 26, outline);
        for (int x = 19; x <= 29; x += 2)
        {
            tex.SetPixel(x, 27, toothWhite); // Dientes superiores
            tex.SetPixel(x + 1, 25, toothWhite); // Dientes inferiores
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/PlasticGolem.png");
    }

    // =========================================================================
    // 🌊 4. FONDO DEL MAR DE PLÁSTICO (256x128 PANORÁMICO HD)
    // =========================================================================
    private static void GenerateOceanBackground()
    {
        int w = 256, h = 128;
        Texture2D tex = CreateClearTexture(w, h);

        Color skyTop = new Color(0.05f, 0.04f, 0.12f);    // Noche cósmica
        Color skyMid = new Color(0.12f, 0.08f, 0.22f);    // Púrpura crepuscular
        Color skyBottom = new Color(0.08f, 0.22f, 0.35f); // Horizonte marino
        Color oceanSurface = new Color(0.12f, 0.45f, 0.65f); // Olas de superficie
        Color oceanDeep = new Color(0.02f, 0.08f, 0.16f);    // Abismo oceánico
        Color shipSilhouette = new Color(0.04f, 0.07f, 0.12f); // Barcos hundidos
        Color moonGlow = new Color(0.65f, 0.85f, 1f, 0.25f);
        Color moonBase = new Color(0.94f, 0.96f, 1f);
        Color moonCrater = new Color(0.82f, 0.86f, 0.92f);

        // 1. Degradado del Cielo
        for (int x = 0; x < w; x++)
        {
            for (int y = 45; y < h; y++)
            {
                float t = (float)(y - 45) / (h - 45);
                Color skyColor = (t > 0.5f) ? Color.Lerp(skyMid, skyTop, (t - 0.5f) * 2f) : Color.Lerp(skyBottom, skyMid, t * 2f);
                tex.SetPixel(x, y, skyColor);
            }
        }

        // 2. Estrellas titilantes
        int[] starsX = { 15, 38, 72, 95, 120, 145, 168, 192, 215, 238, 55, 130, 205, 80, 175 };
        int[] starsY = { 115, 120, 110, 122, 114, 118, 124, 112, 116, 121, 98, 102, 96, 88, 92 };
        for (int i = 0; i < starsX.Length; i++)
        {
            tex.SetPixel(starsX[i], starsY[i], new Color(1f, 1f, 1f, 0.85f));
            if (i % 3 == 0) tex.SetPixel(starsX[i] + 1, starsY[i], new Color(0.6f, 0.9f, 1f, 0.4f));
        }

        // 3. Luna Llena Resplandeciente (Arriba Izquierda)
        int moonCX = 48, moonCY = 100, moonR = 14;
        for (int x = moonCX - moonR - 6; x <= moonCX + moonR + 6; x++)
        {
            for (int y = moonCY - moonR - 6; y <= moonCY + moonR + 6; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(moonCX, moonCY));
                if (dist <= moonR)
                {
                    // Manchas de cráteres lunares
                    bool isCrater = (Mathf.Sin(x * 0.6f) * Mathf.Cos(y * 0.6f) > 0.3f);
                    tex.SetPixel(x, y, isCrater ? moonCrater : moonBase);
                }
                else if (dist <= moonR + 5)
                {
                    // Halo lunar suave
                    tex.SetPixel(x, y, Color.Lerp(moonGlow, tex.GetPixel(x, y), (dist - moonR) / 5f));
                }
            }
        }

        // 4. Siluetas de Barcos Petroleros Hundidos y Plataformas Petrolíferas
        for (int x = 110; x <= 150; x++)
        {
            for (int y = 45; y <= 56; y++)
            {
                if (y <= 50 || (x >= 120 && x <= 135)) tex.SetPixel(x, y, shipSilhouette);
            }
        }
        // Luces de advertencia rojas en las torres distantes
        tex.SetPixel(128, 57, Color.red);
        tex.SetPixel(185, 59, Color.red);
        tex.SetPixel(220, 55, Color.red);

        // 5. Océano Profundo y Olas en la Superficie
        for (int x = 0; x < w; x++)
        {
            int waveY = 44 + (int)(Mathf.Sin(x * 0.12f) * 2.5f + Mathf.Sin(x * 0.05f) * 1.5f);
            for (int y = 0; y <= waveY; y++)
            {
                float depthRatio = (float)y / waveY;
                if (y >= waveY - 1)
                {
                    // Espuma blanca de las olas
                    tex.SetPixel(x, y, Color.white);
                }
                else if (y >= waveY - 3)
                {
                    tex.SetPixel(x, y, oceanSurface);
                }
                else
                {
                    tex.SetPixel(x, y, Color.Lerp(oceanDeep, oceanSurface, depthRatio));
                }
            }
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/OceanBackground.png", 10, true);
    }

    // =========================================================================
    // 🧱 5. PLATAFORMA DE RESIDUOS / CHATARRA (32x32 HD)
    // =========================================================================
    private static void GenerateTrashPlatform()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color rimCyan = new Color(0.15f, 0.9f, 0.95f);   // Borde superior brillante
        Color steelPlate = new Color(0.22f, 0.28f, 0.35f); // Placa de acero
        Color steelDark = new Color(0.14f, 0.18f, 0.24f);  // Sombra de acero
        Color hazardYellow = new Color(0.95f, 0.8f, 0.1f); // Franjas industriales
        Color hazardBlack = new Color(0.1f, 0.12f, 0.16f);
        Color rivet = new Color(0.85f, 0.9f, 0.95f);       // Remaches

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (y >= 30)
                {
                    // Borde metálico superior reflectante
                    tex.SetPixel(x, y, (y == 31) ? Color.white : rimCyan);
                }
                else if (y >= 18)
                {
                    // Zona de franjas de precaución diagonal
                    tex.SetPixel(x, y, ((x + y) % 8 < 4) ? hazardYellow : hazardBlack);
                }
                else if (y >= 6)
                {
                    // Cuerpo de viga de acero
                    tex.SetPixel(x, y, (x % 16 == 0 || y == 6) ? steelDark : steelPlate);
                }
                else
                {
                    // Parte inferior con pernos
                    tex.SetPixel(x, y, steelDark);
                }
            }
        }
        // Remaches metálicos en las esquinas
        tex.SetPixel(2, 28, rivet); tex.SetPixel(29, 28, rivet);
        tex.SetPixel(2, 8, rivet);  tex.SetPixel(29, 8, rivet);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/TrashPlatform.png");
    }

    // =========================================================================
    // ☣️ 6. AGUA TÓXICA (32x32 HD)
    // =========================================================================
    private static void GenerateToxicWater()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color foamGreen = new Color(0.7f, 1f, 0.2f);
        Color toxicMid = new Color(0.12f, 0.65f, 0.45f);
        Color toxicDeep = new Color(0.08f, 0.25f, 0.35f);
        Color bubble = new Color(0.85f, 1f, 0.5f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (y >= 28)
                {
                    tex.SetPixel(x, y, (y == 31 || (x % 5 == 0)) ? foamGreen : bubble);
                }
                else
                {
                    float t = (float)y / 28f;
                    tex.SetPixel(x, y, Color.Lerp(toxicDeep, toxicMid, t));
                }
            }
        }
        // Burbujas tóxicas
        tex.SetPixel(6, 20, bubble); tex.SetPixel(7, 20, bubble);
        tex.SetPixel(18, 14, bubble); tex.SetPixel(19, 14, bubble);
        tex.SetPixel(26, 22, bubble);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/ToxicWater.png");
    }

    // =========================================================================
    // 🌱 7. SEMILLA DE ENERGÍA (24x24 HD)
    // =========================================================================
    private static void GenerateEnergySeed()
    {
        int w = 24, h = 24;
        Texture2D tex = CreateClearTexture(w, h);

        Color goldOuter = new Color(0.9f, 0.65f, 0.05f);
        Color goldCore = new Color(1f, 0.92f, 0.25f);
        Color sprout = new Color(0.2f, 0.95f, 0.4f);
        Color sparkle = Color.white;

        // Semilla ovalada brillante
        for (int x = 7; x <= 16; x++)
        {
            for (int y = 5; y <= 16; y++)
            {
                float dx = (x - 11.5f) / 4.5f;
                float dy = (y - 10.5f) / 5.5f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    tex.SetPixel(x, y, (dx * dx + dy * dy < 0.35f) ? goldCore : goldOuter);
                }
            }
        }
        // Brillo especular
        tex.SetPixel(10, 13, sparkle); tex.SetPixel(11, 13, sparkle);

        // Brote verde milagroso en la cúspide
        tex.SetPixel(11, 17, sprout); tex.SetPixel(12, 17, sprout);
        tex.SetPixel(10, 18, sprout); tex.SetPixel(13, 19, sprout);
        tex.SetPixel(14, 18, sprout);

        // Destellos estelares (sparkles)
        tex.SetPixel(4, 11, sparkle); tex.SetPixel(19, 11, sparkle);
        tex.SetPixel(11, 2, sparkle); tex.SetPixel(11, 22, sparkle);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/EnergySeed.png");
    }

    // =========================================================================
    // 🗡️ 8. DAGA DE KIWI (28x14 HD)
    // =========================================================================
    private static void GenerateKiwiDagger()
    {
        int w = 28, h = 14;
        Texture2D tex = CreateClearTexture(w, h);

        Color bladeSteel = new Color(0.85f, 0.95f, 1f);
        Color bladeEdge = Color.white;
        Color bladeShine = new Color(0.2f, 0.85f, 1f);
        Color guard = new Color(0.95f, 0.75f, 0.15f); // Guardia dorada
        Color hilt = new Color(0.45f, 0.25f, 0.12f);  // Empuñadura cuero

        // Hoja puntiaguda hacia la derecha
        for (int x = 8; x <= 26; x++)
        {
            int halfH = (int)((27 - x) * 0.28f);
            for (int y = 7 - halfH; y <= 7 + halfH; y++)
            {
                if (y == 7) tex.SetPixel(x, y, bladeShine); // Canal de energía central
                else if (y == 7 - halfH || y == 7 + halfH) tex.SetPixel(x, y, bladeEdge);
                else tex.SetPixel(x, y, bladeSteel);
            }
        }

        // Guardia dorada
        for (int y = 3; y <= 11; y++)
        {
            tex.SetPixel(7, y, guard);
            tex.SetPixel(8, y, guard);
        }

        // Empuñadura
        for (int x = 2; x <= 6; x++)
        {
            for (int y = 6; y <= 8; y++) tex.SetPixel(x, y, hilt);
        }
        // Pomo
        tex.SetPixel(1, 7, guard);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/KiwiDagger.png");
    }

    // =========================================================================
    // 🐧 9. PINGÜINO COMPAÑERO (32x32 HD)
    // =========================================================================
    private static void GeneratePenguinCompanion()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color feathers = new Color(0.1f, 0.14f, 0.22f);
        Color whiteBelly = new Color(0.95f, 0.97f, 1f);
        Color beakOrange = new Color(1f, 0.55f, 0.1f);
        Color goggleBrass = new Color(0.95f, 0.75f, 0.15f);
        Color goggleGlass = new Color(0.2f, 0.85f, 0.95f);
        Color scarf = new Color(0.1f, 0.75f, 0.65f);

        // Cuerpo regordete
        for (int x = 8; x <= 23; x++)
        {
            for (int y = 4; y <= 24; y++)
            {
                float dx = (x - 15.5f) / 7.5f;
                float dy = (y - 14f) / 10f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    tex.SetPixel(x, y, (x >= 12 && x <= 19 && y <= 18) ? whiteBelly : feathers);
                }
            }
        }

        // Patitas naranjas
        tex.SetPixel(11, 2, beakOrange); tex.SetPixel(12, 2, beakOrange); tex.SetPixel(13, 2, beakOrange);
        tex.SetPixel(18, 2, beakOrange); tex.SetPixel(19, 2, beakOrange); tex.SetPixel(20, 2, beakOrange);

        // Bufanda turquesa
        for (int x = 9; x <= 22; x++)
        {
            tex.SetPixel(x, 15, scarf);
            tex.SetPixel(x, 16, scarf);
        }

        // Gafas de aviador sobre la frente
        for (int y = 23; y <= 26; y++)
        {
            // Lente izquierdo
            tex.SetPixel(11, y, goggleBrass); tex.SetPixel(14, y, goggleBrass);
            tex.SetPixel(12, y, goggleGlass); tex.SetPixel(13, y, goggleGlass);
            // Lente derecho
            tex.SetPixel(17, y, goggleBrass); tex.SetPixel(20, y, goggleBrass);
            tex.SetPixel(18, y, goggleGlass); tex.SetPixel(19, y, goggleGlass);
        }
        tex.SetPixel(15, 25, goggleBrass); tex.SetPixel(16, 25, goggleBrass); // Puente gafas

        // Ojitos
        tex.SetPixel(13, 20, Color.black);
        tex.SetPixel(18, 20, Color.black);

        // Pico puntiagudo
        tex.SetPixel(15, 18, beakOrange); tex.SetPixel(16, 18, beakOrange);
        tex.SetPixel(15, 17, beakOrange); tex.SetPixel(16, 17, beakOrange);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/PenguinCompanion.png");
    }

    // =========================================================================
    // 🪰 10. MOSCA DE BASURA / DRON (24x24 HD)
    // =========================================================================
    private static void GenerateTrashFly()
    {
        int w = 24, h = 24;
        Texture2D tex = CreateClearTexture(w, h);

        Color metalChassis = new Color(0.28f, 0.32f, 0.38f);
        Color eyeRed = new Color(1f, 0.15f, 0.2f);
        Color wingCyan = new Color(0.4f, 0.9f, 1f, 0.65f);
        Color stinger = new Color(0.9f, 0.7f, 0.1f);

        // Alas zumbantes translúcidas
        for (int x = 4; x <= 19; x++)
        {
            for (int y = 14; y <= 21; y++)
            {
                if ((x + y) % 3 != 0) tex.SetPixel(x, y, wingCyan);
            }
        }

        // Chasis del dron
        for (int x = 8; x <= 15; x++)
        {
            for (int y = 6; y <= 14; y++)
            {
                tex.SetPixel(x, y, metalChassis);
            }
        }

        // Ojo óptico rojo
        tex.SetPixel(11, 10, eyeRed); tex.SetPixel(12, 10, eyeRed);
        tex.SetPixel(11, 9, eyeRed);  tex.SetPixel(12, 9, eyeRed);

        // Aguijón de descarga
        tex.SetPixel(11, 4, stinger); tex.SetPixel(12, 4, stinger);
        tex.SetPixel(11, 3, stinger);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/TrashFly.png");
    }

    // =========================================================================
    // ❤️ 11. CORAZONES DE VIDA (24x24 HD FACETADOS)
    // =========================================================================
    private static void GenerateHeartFull()
    {
        int w = 24, h = 24;
        Texture2D tex = CreateClearTexture(w, h);

        Color goldRim = new Color(0.95f, 0.8f, 0.15f);
        Color rubyDark = new Color(0.65f, 0.08f, 0.18f);
        Color rubyMid = new Color(0.92f, 0.18f, 0.25f);
        Color rubyLight = new Color(1f, 0.45f, 0.55f);
        Color glint = Color.white;

        for (int x = 2; x <= 21; x++)
        {
            for (int y = 2; y <= 21; y++)
            {
                float nx = (x - 11.5f) / 9f;
                float ny = (y - 12.5f) / 9f;
                float a = nx * nx + ny * ny - 1f;
                if (a * a * a - nx * nx * ny * ny * ny <= 0.0f)
                {
                    if (y >= 16 && x <= 9) tex.SetPixel(x, y, (x == 7 && y == 18) ? glint : rubyLight);
                    else if (x > 14 || y < 8) tex.SetPixel(x, y, rubyDark);
                    else tex.SetPixel(x, y, rubyMid);
                }
            }
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/HeartFull.png");
    }

    private static void GenerateHeartEmpty()
    {
        int w = 24, h = 24;
        Texture2D tex = CreateClearTexture(w, h);

        Color frameDark = new Color(0.2f, 0.24f, 0.3f);
        Color frameMid = new Color(0.35f, 0.4f, 0.48f);
        Color hollow = new Color(0.08f, 0.1f, 0.14f, 0.6f);

        for (int x = 2; x <= 21; x++)
        {
            for (int y = 2; y <= 21; y++)
            {
                float nx = (x - 11.5f) / 9f;
                float ny = (y - 12.5f) / 9f;
                float a = nx * nx + ny * ny - 1f;
                if (a * a * a - nx * nx * ny * ny * ny <= 0.0f)
                {
                    tex.SetPixel(x, y, (x == 2 || x == 21 || y == 2) ? frameDark : (y > 18 ? frameMid : hollow));
                }
            }
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/HeartEmpty.png");
    }

    // =========================================================================
    // 🪨 12. TÓTEM DE DESCANSO (28x48 HD)
    // =========================================================================
    private static void GenerateRestTotem()
    {
        int w = 28, h = 48;
        Texture2D tex = CreateClearTexture(w, h);

        Color stoneDark = new Color(0.18f, 0.22f, 0.28f);
        Color stoneMid = new Color(0.32f, 0.38f, 0.45f);
        Color stoneLight = new Color(0.48f, 0.55f, 0.65f);
        Color runeCyan = new Color(0.1f, 0.95f, 0.9f);
        Color vineGreen = new Color(0.25f, 0.75f, 0.35f);

        for (int y = 4; y <= 44; y++)
        {
            int halfW = (y < 8) ? 12 : (y > 40 ? 8 : 10);
            for (int x = 14 - halfW; x <= 14 + halfW; x++)
            {
                if (x == 14 - halfW || x == 14 + halfW || y == 4 || y == 44) tex.SetPixel(x, y, stoneDark);
                else if (x > 14) tex.SetPixel(x, y, stoneLight);
                else tex.SetPixel(x, y, stoneMid);
            }
        }

        int[] runeY = { 16, 22, 28, 34 };
        foreach (int ry in runeY)
        {
            tex.SetPixel(13, ry, runeCyan); tex.SetPixel(14, ry, Color.white); tex.SetPixel(15, ry, runeCyan);
            tex.SetPixel(14, ry + 1, runeCyan); tex.SetPixel(14, ry - 1, runeCyan);
        }

        for (int y = 6; y <= 38; y += 2)
        {
            int vx = 14 + (int)(Mathf.Sin(y * 0.3f) * 8f);
            tex.SetPixel(vx, y, vineGreen);
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/RestTotem.png");
    }

    // =========================================================================
    // 🌀 13. VÓRTICE DE PORTAL (40x40 HD)
    // =========================================================================
    private static void GeneratePortalVortex()
    {
        int w = 40, h = 40;
        Texture2D tex = CreateClearTexture(w, h);

        Color portalRim = new Color(0.65f, 0.25f, 0.95f);
        Color portalMid = new Color(0.35f, 0.75f, 1f);
        Color portalCore = Color.white;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(20, 20));
                if (dist <= 18)
                {
                    float angle = Mathf.Atan2(y - 20, x - 20);
                    float spiral = Mathf.Sin(angle * 3f + dist * 0.5f);
                    if (dist < 4) tex.SetPixel(x, y, portalCore);
                    else if (spiral > 0) tex.SetPixel(x, y, portalMid);
                    else tex.SetPixel(x, y, portalRim);
                }
            }
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/PortalVortex.png");
    }

    // =========================================================================
    // 🦉 14. BÚHO GUÍA (CIUDAD DE HUMO 32x32 HD)
    // =========================================================================
    private static void GenerateOwlCompanion()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color feathers = new Color(0.45f, 0.32f, 0.24f);
        Color feathersLight = new Color(0.65f, 0.48f, 0.36f);
        Color belly = new Color(0.9f, 0.85f, 0.75f);
        Color eyeGold = new Color(1f, 0.85f, 0.1f);
        Color beak = new Color(0.95f, 0.6f, 0.1f);
        Color scarf = new Color(0.2f, 0.7f, 0.85f);

        for (int x = 8; x <= 23; x++)
        {
            for (int y = 6; y <= 24; y++)
            {
                float dx = (x - 15.5f) / 7.5f;
                float dy = (y - 15f) / 9f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    tex.SetPixel(x, y, (x >= 12 && x <= 19 && y <= 16) ? belly : feathers);
                }
            }
        }
        tex.SetPixel(10, 25, feathers); tex.SetPixel(11, 26, feathersLight);
        tex.SetPixel(21, 25, feathers); tex.SetPixel(20, 26, feathersLight);

        tex.SetPixel(12, 19, eyeGold); tex.SetPixel(13, 19, eyeGold); tex.SetPixel(13, 19, Color.black);
        tex.SetPixel(18, 19, eyeGold); tex.SetPixel(19, 19, eyeGold); tex.SetPixel(18, 19, Color.black);
        tex.SetPixel(15, 17, beak); tex.SetPixel(16, 17, beak); tex.SetPixel(15, 16, beak);

        for (int x = 10; x <= 21; x++) tex.SetPixel(x, 14, scarf);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/OwlCompanion.png");
    }

    // =========================================================================
    // ☁️ 15. NUBE TÓXICA (32x32 HD)
    // =========================================================================
    private static void GenerateToxicCloud()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color cloudMid = new Color(0.48f, 0.28f, 0.58f, 0.9f);
        Color cloudPoison = new Color(0.6f, 0.9f, 0.25f);
        Color eyes = Color.red;

        for (int x = 4; x <= 27; x++)
        {
            for (int y = 6; y <= 25; y++)
            {
                float d1 = Vector2.Distance(new Vector2(x, y), new Vector2(12, 15));
                float d2 = Vector2.Distance(new Vector2(x, y), new Vector2(20, 16));
                float d3 = Vector2.Distance(new Vector2(x, y), new Vector2(16, 20));
                if (d1 < 7 || d2 < 7 || d3 < 6)
                {
                    tex.SetPixel(x, y, ((x + y) % 5 == 0) ? cloudPoison : cloudMid);
                }
            }
        }
        tex.SetPixel(12, 16, eyes); tex.SetPixel(13, 16, eyes);
        tex.SetPixel(19, 16, eyes); tex.SetPixel(20, 16, eyes);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/ToxicCloud.png");
    }

    // =========================================================================
    // 🏭 16. PLATAFORMA DE FÁBRICA / CIUDAD (32x32 HD)
    // =========================================================================
    private static void GenerateFactoryPlatform()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color metalEdge = new Color(0.7f, 0.75f, 0.85f);
        Color grateDark = new Color(0.15f, 0.16f, 0.2f);
        Color grateLight = new Color(0.35f, 0.38f, 0.45f);
        Color rust = new Color(0.55f, 0.32f, 0.18f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (y >= 29) tex.SetPixel(x, y, metalEdge);
                else if (y >= 14)
                {
                    tex.SetPixel(x, y, (x % 4 == 0 || y % 4 == 0) ? grateDark : grateLight);
                }
                else
                {
                    tex.SetPixel(x, y, ((x + y) % 7 == 0) ? rust : grateDark);
                }
            }
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/FactoryPlatform.png");
    }

    // =========================================================================
    // 💨 17. FONDO DE SMOG (CIUDAD DE HUMO 256x128 HD)
    // =========================================================================
    private static void GenerateSmogBackground()
    {
        int w = 256, h = 128;
        Texture2D tex = CreateClearTexture(w, h);

        Color skyTop = new Color(0.14f, 0.08f, 0.18f);
        Color skyBottom = new Color(0.38f, 0.18f, 0.12f);
        Color factoryDark = new Color(0.08f, 0.06f, 0.1f);
        Color glowOrange = new Color(1f, 0.5f, 0.1f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                tex.SetPixel(x, y, Color.Lerp(skyBottom, skyTop, t));
            }
        }

        int[] chimX = { 30, 75, 120, 160, 210 };
        int[] chimH = { 70, 85, 65, 90, 75 };
        for (int i = 0; i < chimX.Length; i++)
        {
            int cx = chimX[i];
            for (int x = cx - 6; x <= cx + 6; x++)
            {
                for (int y = 0; y <= chimH[i]; y++)
                {
                    tex.SetPixel(x, y, factoryDark);
                }
            }
            tex.SetPixel(cx, chimH[i] + 1, glowOrange);
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/SmogBackground.png", 10, true);
    }

    // =========================================================================
    // 🔥 18. SLIME DE FUEGO (28x24 HD)
    // =========================================================================
    private static void GenerateFireSlime()
    {
        int w = 28, h = 24;
        Texture2D tex = CreateClearTexture(w, h);

        Color magmaCore = new Color(1f, 0.95f, 0.4f);
        Color flameMid = new Color(1f, 0.45f, 0.1f);
        Color flameDark = new Color(0.7f, 0.15f, 0.05f);

        for (int x = 4; x <= 23; x++)
        {
            for (int y = 2; y <= 20; y++)
            {
                float dx = (x - 13.5f) / 9.5f;
                float dy = (y - 9f) / 8f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    tex.SetPixel(x, y, (dx * dx + dy * dy < 0.35f) ? magmaCore : flameMid);
                }
            }
        }
        tex.SetPixel(13, 21, flameDark); tex.SetPixel(14, 22, magmaCore); tex.SetPixel(15, 21, flameDark);
        tex.SetPixel(11, 10, Color.black);
        tex.SetPixel(17, 10, Color.black);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/FireSlime.png");
    }

    // =========================================================================
    // 🧱 19. MURO DE FUEGO (24x48 HD)
    // =========================================================================
    private static void GenerateFireBarrier()
    {
        int w = 24, h = 48;
        Texture2D tex = CreateClearTexture(w, h);

        Color fireHot = new Color(1f, 0.95f, 0.5f);
        Color fireMid = new Color(1f, 0.45f, 0.1f);
        Color fireDark = new Color(0.75f, 0.15f, 0.05f);

        for (int x = 2; x <= 21; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float noise = Mathf.Sin(x * 0.8f + y * 0.4f);
                tex.SetPixel(x, y, (noise > 0.4f) ? fireHot : (noise > -0.2f ? fireMid : fireDark));
            }
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/FireBarrier.png");
    }

    // =========================================================================
    // 🦝 20. MAPACHE INVENTOR / TIENDA (32x32 HD)
    // =========================================================================
    private static void GenerateRaccoonShop()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color furGrey = new Color(0.45f, 0.48f, 0.52f);
        Color maskBlack = new Color(0.12f, 0.12f, 0.15f);
        Color nose = Color.black;
        Color apron = new Color(0.55f, 0.35f, 0.15f);

        for (int x = 8; x <= 23; x++)
        {
            for (int y = 4; y <= 24; y++)
            {
                float dx = (x - 15.5f) / 7.5f;
                float dy = (y - 14f) / 9f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    tex.SetPixel(x, y, (y <= 14) ? apron : furGrey);
                }
            }
        }
        for (int x = 11; x <= 20; x++) tex.SetPixel(x, 20, maskBlack);
        tex.SetPixel(13, 20, Color.white);
        tex.SetPixel(18, 20, Color.white);
        tex.SetPixel(15, 18, nose); tex.SetPixel(16, 18, nose);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/RaccoonShop.png");
    }

    // =========================================================================
    // 🏜️ 21. FONDO DEL DESIERTO (256x128 PANORÁMICO HD)
    // =========================================================================
    private static void GenerateDesertBackground()
    {
        int w = 256, h = 128;
        Texture2D tex = CreateClearTexture(w, h);

        Color skyTop = new Color(0.28f, 0.12f, 0.08f);
        Color skyBottom = new Color(0.85f, 0.45f, 0.15f);
        Color duneFar = new Color(0.45f, 0.22f, 0.12f);
        Color duneNear = new Color(0.25f, 0.11f, 0.06f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 40; y < h; y++)
            {
                float t = (float)(y - 40) / (h - 40);
                tex.SetPixel(x, y, Color.Lerp(skyBottom, skyTop, t));
            }
        }

        int sunCX = 180, sunCY = 85, sunR = 18;
        for (int x = sunCX - sunR; x <= sunCX + sunR; x++)
        {
            for (int y = sunCY - sunR; y <= sunCY + sunR; y++)
            {
                if (Vector2.Distance(new Vector2(x, y), new Vector2(sunCX, sunCY)) <= sunR)
                {
                    tex.SetPixel(x, y, new Color(1f, 0.9f, 0.3f));
                }
            }
        }

        for (int x = 0; x < w; x++)
        {
            int dY = 38 + (int)(Mathf.Sin(x * 0.05f) * 8f);
            for (int y = 0; y <= dY; y++)
            {
                tex.SetPixel(x, y, (y > dY - 4) ? duneFar : duneNear);
            }
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/DesertBackground.png", 10, true);
    }

    // =========================================================================
    // ⚙️ 22. PLATAFORMA DE CHATARRA (32x32 HD)
    // =========================================================================
    private static void GenerateScrapPlatform()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color metalRust = new Color(0.55f, 0.35f, 0.2f);
        Color metalBright = new Color(0.8f, 0.55f, 0.3f);
        Color rivet = new Color(0.95f, 0.85f, 0.6f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (y >= 29) tex.SetPixel(x, y, metalBright);
                else tex.SetPixel(x, y, ((x + y) % 6 == 0) ? metalRust : new Color(0.35f, 0.22f, 0.15f));
            }
        }
        tex.SetPixel(2, 27, rivet); tex.SetPixel(29, 27, rivet);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/ScrapPlatform.png");
    }

    // =========================================================================
    // 🦊 23. ZORRO COMPAÑERO (BOSQUE MARCHITO 32x32 HD)
    // =========================================================================
    private static void GenerateFoxCompanion()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color foxRed = new Color(0.92f, 0.45f, 0.15f);
        Color foxWhite = new Color(0.96f, 0.96f, 1f);
        Color foxDark = new Color(0.45f, 0.18f, 0.08f);

        for (int x = 4; x <= 10; x++)
        {
            for (int y = 8; y <= 20; y++)
            {
                tex.SetPixel(x, y, (y > 16) ? foxWhite : foxRed);
            }
        }
        for (int x = 10; x <= 22; x++)
        {
            for (int y = 4; y <= 22; y++)
            {
                tex.SetPixel(x, y, (x >= 14 && x <= 18 && y <= 14) ? foxWhite : foxRed);
            }
        }
        tex.SetPixel(13, 27, foxDark); tex.SetPixel(14, 28, foxRed);
        tex.SetPixel(21, 27, foxDark); tex.SetPixel(20, 28, foxRed);
        tex.SetPixel(15, 20, Color.black);
        tex.SetPixel(19, 20, Color.black);
        tex.SetPixel(17, 18, Color.black);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/FoxCompanion.png");
    }

    // =========================================================================
    // 🪵 24. ESPECTRO DE RAMAS (32x40 HD)
    // =========================================================================
    private static void GenerateBranchSpecter()
    {
        int w = 32, h = 40;
        Texture2D tex = CreateClearTexture(w, h);

        Color barkDark = new Color(0.24f, 0.16f, 0.1f);
        Color barkLight = new Color(0.42f, 0.28f, 0.18f);
        Color sapGreen = new Color(0.3f, 0.95f, 0.4f);

        for (int x = 8; x <= 23; x++)
        {
            for (int y = 4; y <= 32; y++)
            {
                tex.SetPixel(x, y, (x % 3 == 0 || y % 5 == 0) ? barkDark : barkLight);
            }
        }
        tex.SetPixel(8, 33, barkDark); tex.SetPixel(7, 34, barkLight); tex.SetPixel(6, 35, barkLight);
        tex.SetPixel(23, 33, barkDark); tex.SetPixel(24, 34, barkLight); tex.SetPixel(25, 35, barkLight);

        tex.SetPixel(12, 24, sapGreen); tex.SetPixel(13, 24, sapGreen);
        tex.SetPixel(18, 24, sapGreen); tex.SetPixel(19, 24, sapGreen);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/BranchSpecter.png");
    }

    // =========================================================================
    // 🌲 25. FONDO DEL BOSQUE MARCHITO (256x128 PANORÁMICO HD)
    // =========================================================================
    private static void GenerateForestBackground()
    {
        int w = 256, h = 128;
        Texture2D tex = CreateClearTexture(w, h);

        Color skyTop = new Color(0.06f, 0.1f, 0.08f);
        Color skyBottom = new Color(0.18f, 0.28f, 0.16f);
        Color treeFar = new Color(0.08f, 0.14f, 0.1f);
        Color treeNear = new Color(0.04f, 0.08f, 0.05f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                tex.SetPixel(x, y, Color.Lerp(skyBottom, skyTop, t));
            }
        }

        for (int x = 0; x < w; x++)
        {
            if (x % 32 < 6)
            {
                for (int y = 0; y < 95; y++) tex.SetPixel(x, y, treeFar);
            }
            if (x % 54 < 8)
            {
                for (int y = 0; y < 115; y++) tex.SetPixel(x, y, treeNear);
            }
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/ForestBackground.png", 10, true);
    }

    // =========================================================================
    // 🪵 26. PLATAFORMA DE MADERA (32x32 HD)
    // =========================================================================
    private static void GenerateWoodPlatform()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color mossGreen = new Color(0.2f, 0.75f, 0.35f);
        Color woodMid = new Color(0.42f, 0.28f, 0.16f);
        Color woodDark = new Color(0.25f, 0.16f, 0.09f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (y >= 29) tex.SetPixel(x, y, (x % 4 == 0) ? mossGreen : woodMid);
                else tex.SetPixel(x, y, ((x + y) % 6 == 0) ? woodDark : woodMid);
            }
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/WoodPlatform.png");
    }

    // =========================================================================
    // 🌿 27. MURO DE RAÍCES SECAS (24x48 HD)
    // =========================================================================
    private static void GenerateRootBarrier()
    {
        int w = 24, h = 48;
        Texture2D tex = CreateClearTexture(w, h);

        Color rootDark = new Color(0.28f, 0.18f, 0.12f);
        Color rootLight = new Color(0.48f, 0.32f, 0.2f);

        for (int y = 0; y < h; y++)
        {
            int margin = (int)(Mathf.Sin(y * 0.4f) * 2) + 2;
            for (int x = margin; x < w - margin; x++)
            {
                tex.SetPixel(x, y, ((x + y) % 4 == 0) ? rootDark : rootLight);
            }
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/RootBarrier.png");
    }

    // =========================================================================
    // 🫖 28. JEFE: LA COLMENA TÓXICA (64x64 HD)
    // =========================================================================
    private static void GenerateColmenaBoss()
    {
        int w = 64, h = 64;
        Texture2D tex = CreateClearTexture(w, h);

        Color ironDark = new Color(0.2f, 0.22f, 0.25f);
        Color ironRust = new Color(0.45f, 0.25f, 0.15f);
        Color fireCore = new Color(1f, 0.7f, 0.15f);
        Color fireHot = new Color(1f, 0.95f, 0.5f);

        for (int x = 8; x <= 55; x++)
        {
            for (int y = 6; y <= 50; y++)
            {
                float dx = (x - 31.5f) / 23.5f;
                float dy = (y - 28f) / 22f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    tex.SetPixel(x, y, ((x + y) % 6 == 0) ? ironRust : ironDark);
                }
            }
        }

        for (int x = 20; x <= 43; x++)
        {
            for (int y = 18; y <= 34; y++)
            {
                float dx = (x - 31.5f) / 11.5f;
                float dy = (y - 26f) / 8f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    tex.SetPixel(x, y, (dy < 0) ? fireHot : fireCore);
                }
            }
        }
        for (int y = 50; y <= 60; y++)
        {
            for (int x = 18; x <= 22; x++) tex.SetPixel(x, y, ironDark);
            for (int x = 41; x <= 45; x++) tex.SetPixel(x, y, ironDark);
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/ColmenaBoss.png");
    }

    // =========================================================================
    // 🛡️ 29. TORRECILLA BLINDADA (32x32 HD)
    // =========================================================================
    private static void GenerateScrapTurret()
    {
        int w = 32, h = 32;
        Texture2D tex = CreateClearTexture(w, h);

        Color metal = new Color(0.35f, 0.38f, 0.44f);
        Color shieldCyan = new Color(0.15f, 0.85f, 0.95f);
        Color eyeRed = Color.red;

        for (int x = 6; x <= 22; x++)
        {
            for (int y = 4; y <= 24; y++) tex.SetPixel(x, y, metal);
        }
        for (int y = 4; y <= 27; y++)
        {
            tex.SetPixel(23, y, shieldCyan);
            tex.SetPixel(24, y, Color.white);
            tex.SetPixel(25, y, shieldCyan);
        }
        tex.SetPixel(15, 16, eyeRed); tex.SetPixel(16, 16, eyeRed);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/ScrapTurret.png");
    }

    // =========================================================================
    // 🌟 30. LOGO DEL JUEGO: KIWI GUARDIÁN (192x56 HD)
    // =========================================================================
    private static void GenerateTitleLogo()
    {
        int w = 192, h = 56;
        Texture2D tex = CreateClearTexture(w, h);

        Color goldLight = new Color(1.0f, 0.9f, 0.4f);
        Color goldMid = new Color(0.95f, 0.72f, 0.15f);
        Color goldDark = new Color(0.7f, 0.45f, 0.08f);
        Color shadowCyan = new Color(0.1f, 0.45f, 0.65f);
        Color vineGreen = new Color(0.25f, 0.85f, 0.35f);
        Color gemBlue = new Color(0.2f, 0.85f, 1f);

        // Fondo de estandarte estilizado
        for (int x = 16; x < w - 16; x++)
        {
            for (int y = 6; y < h - 6; y++)
            {
                if (y == 6 || y == h - 7 || x == 16 || x == w - 17)
                    tex.SetPixel(x, y, new Color(0.15f, 0.22f, 0.3f, 0.75f));
            }
        }

        // Trazado de "K I W I" en letras grandes doradas con sombreado cian
        // K
        DrawLogoLetter(tex, 44, 26, new int[,] {
            {1,0,0,1}, {1,0,1,0}, {1,1,0,0}, {1,1,0,0}, {1,0,1,0}, {1,0,0,1}
        }, goldLight, goldMid, shadowCyan);

        // I
        DrawLogoLetter(tex, 68, 26, new int[,] {
            {1,1,1}, {0,1,0}, {0,1,0}, {0,1,0}, {0,1,0}, {1,1,1}
        }, goldLight, goldMid, shadowCyan);

        // W
        DrawLogoLetter(tex, 88, 26, new int[,] {
            {1,0,0,0,1}, {1,0,0,0,1}, {1,0,1,0,1}, {1,0,1,0,1}, {1,1,0,1,1}, {0,1,0,1,0}
        }, goldLight, goldMid, shadowCyan);

        // I
        DrawLogoLetter(tex, 118, 26, new int[,] {
            {1,1,1}, {0,1,0}, {0,1,0}, {0,1,0}, {0,1,0}, {1,1,1}
        }, goldLight, goldMid, shadowCyan);

        // Cristal reluciente sobre la I
        tex.SetPixel(70, 48, gemBlue); tex.SetPixel(70, 49, Color.white);
        tex.SetPixel(120, 48, gemBlue); tex.SetPixel(120, 49, Color.white);

        // Hojas místicas decorativas a los lados
        for (int i = 0; i < 12; i++)
        {
            tex.SetPixel(24 + i, 16 + (i % 3), vineGreen);
            tex.SetPixel(w - 25 - i, 16 + (i % 3), vineGreen);
        }

        // Línea divisoria elegante
        for (int x = 32; x < w - 32; x++)
        {
            tex.SetPixel(x, 20, (x % 4 == 0) ? goldLight : goldDark);
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/TitleLogo.png");
    }

    private static void DrawLogoLetter(Texture2D tex, int startX, int startY, int[,] matrix, Color light, Color mid, Color shadow)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        int scale = 3;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (matrix[rows - 1 - r, c] == 1)
                {
                    for (int sx = 0; sx < scale; sx++)
                    {
                        for (int sy = 0; sy < scale; sy++)
                        {
                            int px = startX + c * scale + sx;
                            int py = startY + r * scale + sy;
                            // Sombra proyectada
                            tex.SetPixel(px + 2, py - 2, shadow);
                            // Relleno degradado
                            tex.SetPixel(px, py, (r > rows / 2) ? light : mid);
                        }
                    }
                }
            }
        }
    }

    // =========================================================================
    // 🌰 31. LA ÚLTIMA SEMILLA GIGANTE (48x48 HD)
    // =========================================================================
    private static void GenerateGiantSeed()
    {
        int w = 48, h = 48;
        Texture2D tex = CreateClearTexture(w, h);

        Color goldOuter = new Color(0.95f, 0.72f, 0.15f);
        Color goldCore = new Color(1.0f, 0.95f, 0.65f);
        Color goldGlint = Color.white;
        Color shellBrown = new Color(0.42f, 0.22f, 0.12f);
        Color sproutGreen = new Color(0.28f, 0.95f, 0.42f);
        Color auraGlow = new Color(0.2f, 0.85f, 1f, 0.35f);

        // Aura luminosa circular suave
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(24, 22));
                if (d < 22f)
                {
                    float a = Mathf.Clamp01(1f - (d / 22f)) * 0.4f;
                    tex.SetPixel(x, y, new Color(auraGlow.r, auraGlow.g, auraGlow.b, a));
                }
            }
        }

        // Cuerpo elipsoidal de la Semilla Sagrada
        for (int x = 12; x <= 36; x++)
        {
            for (int y = 8; y <= 34; y++)
            {
                float dx = (x - 24f) / 10f;
                float dy = (y - 20f) / 12f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    if (dx * dx + dy * dy < 0.35f) tex.SetPixel(x, y, (dy > 0 && dx < 0) ? goldGlint : goldCore);
                    else if (y < 14 || dx < -0.6f) tex.SetPixel(x, y, shellBrown);
                    else tex.SetPixel(x, y, goldOuter);
                }
            }
        }

        // Brote sagrado gemelo en la cúspide
        int[,] leaves = {
            {24, 34}, {24, 35}, {23, 36}, {22, 37}, {21, 38}, {20, 38},
            {24, 34}, {25, 35}, {26, 36}, {27, 37}, {28, 38}, {29, 38}
        };
        for (int i = 0; i < leaves.GetLength(0); i++)
        {
            tex.SetPixel(leaves[i, 0], leaves[i, 1], sproutGreen);
            tex.SetPixel(leaves[i, 0], leaves[i, 1] + 1, (i % 3 == 0) ? Color.white : sproutGreen);
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/GiantSeed.png");
    }

    // =========================================================================
    // 📖 32. SLIDES NARRATIVOS DEL PRÓLOGO (160x90 HD)
    // =========================================================================
    private static void GeneratePrologueSlides()
    {
        int w = 160, h = 90;

        // Slide 1: El Mundo en Armonía
        Texture2D s1 = CreateClearTexture(w, h);
        Color skyBlue = new Color(0.35f, 0.72f, 0.95f);
        Color hillGreen = new Color(0.2f, 0.65f, 0.3f);
        Color treeTrunk = new Color(0.4f, 0.25f, 0.15f);
        Color foliage = new Color(0.28f, 0.85f, 0.42f);
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (y > 35) s1.SetPixel(x, y, Color.Lerp(skyBlue, Color.white, (float)(y - 35) / (h - 35)));
                else s1.SetPixel(x, y, hillGreen);
            }
        }
        // Árbol ancestral en el centro
        for (int x = 76; x <= 84; x++) for (int y = 30; y <= 55; y++) s1.SetPixel(x, y, treeTrunk);
        for (int x = 60; x <= 100; x++)
        {
            for (int y = 50; y <= 82; y++)
            {
                if (Vector2.Distance(new Vector2(x, y), new Vector2(80, 66)) < 18f) s1.SetPixel(x, y, foliage);
            }
        }
        s1.Apply();
        SaveSprite(s1, "Assets/Sprites/PrologueSlide1.png", 10, true);

        // Slide 2: La Gran Contaminación y los Residuos
        Texture2D s2 = CreateClearTexture(w, h);
        Color skyPolluted = new Color(0.22f, 0.1f, 0.18f);
        Color acidWater = new Color(0.12f, 0.58f, 0.35f);
        Color factoryDark = new Color(0.08f, 0.08f, 0.1f);
        Color toxicYellow = new Color(0.85f, 0.78f, 0.15f);
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (y > 30) s2.SetPixel(x, y, Color.Lerp(skyPolluted, new Color(0.35f, 0.15f, 0.1f), (float)(y - 30) / (h - 30)));
                else s2.SetPixel(x, y, acidWater);
            }
        }
        // Chimeneas humeantes
        int[] chims = { 35, 75, 120 };
        foreach (int cx in chims)
        {
            for (int x = cx - 8; x <= cx + 8; x++)
                for (int y = 25; y <= 65; y++)
                    s2.SetPixel(x, y, factoryDark);
            // Nubes de humo
            for (int x = cx - 14; x <= cx + 14; x++)
                for (int y = 65; y <= 85; y++)
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(cx, 75)) < 12f)
                        s2.SetPixel(x, y, new Color(0.45f, 0.38f, 0.42f, 0.85f));
        }
        s2.Apply();
        SaveSprite(s2, "Assets/Sprites/PrologueSlide2.png", 10, true);

        // Slide 3: La Última Semilla Concentrada
        Texture2D s3 = CreateClearTexture(w, h);
        Color darkCave = new Color(0.06f, 0.05f, 0.08f);
        Color pedestal = new Color(0.25f, 0.28f, 0.35f);
        Color seedRay = new Color(1f, 0.88f, 0.35f, 0.4f);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                s3.SetPixel(x, y, darkCave);
        // Pedestal
        for (int x = 65; x <= 95; x++)
            for (int y = 15; y <= 35; y++)
                s3.SetPixel(x, y, pedestal);
        // Rayo de luz celestial hacia la semilla
        for (int y = 35; y < h; y++)
        {
            int halfSpread = 10 + (h - y) / 3;
            for (int x = 80 - halfSpread; x <= 80 + halfSpread; x++)
                s3.SetPixel(x, y, seedRay);
        }
        s3.Apply();
        SaveSprite(s3, "Assets/Sprites/PrologueSlide3.png", 10, true);

        // Slide 4: Kiwi el Elegido Mirando el Horizonte
        Texture2D s4 = CreateClearTexture(w, h);
        Color twilightSky = new Color(0.12f, 0.08f, 0.24f);
        Color twilightHills = new Color(0.08f, 0.14f, 0.22f);
        Color orangeCat = new Color(0.92f, 0.52f, 0.16f);
        Color tealJacket = new Color(0.12f, 0.45f, 0.48f);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                s4.SetPixel(x, y, (y > 28) ? twilightSky : twilightHills);
        // Silueta de Kiwi de espaldas con mochila brillante
        for (int x = 74; x <= 86; x++)
        {
            for (int y = 25; y <= 45; y++)
            {
                if (y > 38) s4.SetPixel(x, y, orangeCat); // Cabeza con orejitas
                else s4.SetPixel(x, y, tealJacket); // Chaqueta
            }
        }
        // Mochila con cristal azul brillante
        s4.SetPixel(78, 32, Color.cyan); s4.SetPixel(79, 32, Color.white);
        s4.SetPixel(80, 32, Color.cyan); s4.SetPixel(79, 33, Color.cyan);
        s4.Apply();
        SaveSprite(s4, "Assets/Sprites/PrologueSlide4.png", 10, true);
    }
}
#endif
