using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Extractor y Configurador Oficial de Arte HD basado en las capturas del usuario:
/// - Kiwi el Gato Guardián (Idle, Correr, Salto, Ataque, Caminata) con fondo transparente.
/// - Los 4 Fondos Escénicos Elementales (Agua, Aire, Fuego, Tierra).
/// - Los 4 Jefes y Espíritus Corrompidos (Gólem de Basura, Cráneo de Humo, Espectro de Ramas, Slime Ácido).
/// </summary>
public static class UserArtAssetExtractor
{
    private const string ArtSourceDir = "Assets/ArtSource";
    private const string SpritesDir = "Assets/Sprites";

    [MenuItem("Kiwi/🎨 Cargar y Aplicar Arte de las Capturas (HD)", priority = -5)]
    public static void ExtractAndApplyAllArtMenu()
    {
        ExtractAndApplyAllArt(true);
    }

    public static void ExtractAndApplyAllArt(bool showDialog = false)
    {
        Debug.Log("🎨 [Kiwi Art] Iniciando extracción y configuración de Arte HD...");

        if (!Directory.Exists(SpritesDir))
        {
            Directory.CreateDirectory(SpritesDir);
        }

        string kiwiSource = Path.Combine(ArtSourceDir, "KiwiSheetSource.png");
        string regionsSource = Path.Combine(ArtSourceDir, "RegionsPanelsSource.png");
        string bossesSource = Path.Combine(ArtSourceDir, "BossesSheetSource.png");

        if (!File.Exists(kiwiSource) || !File.Exists(regionsSource) || !File.Exists(bossesSource))
        {
            Debug.LogError("❌ No se encontraron los archivos fuente en Assets/ArtSource/");
            return;
        }

        // 1. Extraer Kiwi el Gato
        Texture2D kiwiTex = LoadTextureFromFile(kiwiSource);
        if (kiwiTex != null)
        {
            // Poses de Kiwi (origen en Texture2D: Y=0 abajo)
            SaveCroppedSprite(kiwiTex, 47, 106, 65, 90, "KiwiCat.png", 60, true);
            SaveCroppedSprite(kiwiTex, 263, 291, 95, 90, "KiwiCat_Run.png", 60, true);
            SaveCroppedSprite(kiwiTex, 284, 106, 94, 90, "KiwiCat_Attack.png", 60, true);
            SaveCroppedSprite(kiwiTex, 162, 106, 81, 90, "KiwiCat_Jump.png", 60, true);
            SaveCroppedSprite(kiwiTex, 30, 14, 70, 88, "KiwiCat_Walk1.png", 60, true);
            SaveCroppedSprite(kiwiTex, 118, 14, 72, 88, "KiwiCat_Walk2.png", 60, true);
            SaveCroppedSprite(kiwiTex, 214, 14, 72, 88, "KiwiCat_Walk3.png", 60, true);
            SaveCroppedSprite(kiwiTex, 304, 14, 72, 88, "KiwiCat_Walk4.png", 60, true);
            Object.DestroyImmediate(kiwiTex);
        }

        // 2. Extraer Fondos de las 4 Regiones
        Texture2D regionsTex = LoadTextureFromFile(regionsSource);
        if (regionsTex != null)
        {
            SaveCroppedSprite(regionsTex, 18, 24, 212, 134, "OceanBackground.png", 10, false, true);
            SaveCroppedSprite(regionsTex, 236, 24, 214, 134, "SmogBackground.png", 10, false, true);
            SaveCroppedSprite(regionsTex, 18, 168, 212, 140, "ForestBackground.png", 10, false, true);
            SaveCroppedSprite(regionsTex, 236, 168, 214, 140, "DesertBackground.png", 10, false, true);
            Object.DestroyImmediate(regionsTex);
        }

        // 3. Extraer Jefes y Criaturas
        Texture2D bossesTex = LoadTextureFromFile(bossesSource);
        if (bossesTex != null)
        {
            SaveCroppedSprite(bossesTex, 16, 5, 102, 100, "PlasticGolem.png", 40, true);
            SaveCroppedSprite(bossesTex, 143, 5, 76, 100, "ToxicCloud.png", 40, true);
            SaveCroppedSprite(bossesTex, 246, 5, 68, 100, "BranchSpecter.png", 40, true);
            SaveCroppedSprite(bossesTex, 339, 5, 75, 100, "FireSlime.png", 40, true);
            Object.DestroyImmediate(bossesTex);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✨ [Kiwi Art] ¡Todos los sprites y fondos HD fueron extraídos y configurados con éxito!");
        if (showDialog)
        {
            EditorUtility.DisplayDialog("Kiwi: Arte HD Aplicado", 
                "¡El arte oficial de las capturas se ha extraído y aplicado correctamente!\n\n" +
                "✔ Gato Kiwi (Naranja, Chaqueta Teal, Mochila con Semilla)\n" +
                "✔ Fondos HD de Agua, Aire, Fuego y Tierra\n" +
                "✔ Gólem de Basura, Cráneo de Humo, Espectro de Ramas y Slime Ácido\n\n" +
                "Pulsa 'Reconstruir Todo el Mundo' o pulsa Play (▶) para disfrutar del juego con su nuevo estilo visual.", "¡Entendido!");
        }
    }

    private static Texture2D LoadTextureFromFile(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(bytes);
        return tex;
    }

    private static void SaveCroppedSprite(Texture2D srcTex, int x, int topY, int w, int h, string fileName, int ppu, bool removeBlackBg, bool isTiledBg = false)
    {
        // En Texture2D, Y=0 es abajo. Convertir topY a bottomY:
        int bottomY = srcTex.height - (topY + h);
        if (bottomY < 0) bottomY = 0;

        Color[] pixels = srcTex.GetPixels(x, bottomY, w, h);

        if (removeBlackBg)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                Color p = pixels[i];
                // Si el píxel es negro puro o sombra de fondo (R,G,B <= 0.08)
                if (p.r <= 0.08f && p.g <= 0.08f && p.b <= 0.08f)
                {
                    pixels[i] = new Color(0, 0, 0, 0);
                }
            }
        }

        Texture2D cropped = new Texture2D(w, h, TextureFormat.RGBA32, false);
        cropped.SetPixels(pixels);
        cropped.Apply();

        byte[] pngBytes = cropped.EncodeToPNG();
        Object.DestroyImmediate(cropped);

        string destPath = Path.Combine(SpritesDir, fileName);
        File.WriteAllBytes(destPath, pngBytes);

        // Configurar Importer de Unity
        AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(destPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.spritePixelsPerUnit = ppu;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            if (isTiledBg)
            {
                importer.wrapModeU = TextureWrapMode.Repeat;
                importer.wrapModeV = TextureWrapMode.Clamp;
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
            }
            else
            {
                importer.wrapMode = TextureWrapMode.Clamp;
            }

            importer.SaveAndReimport();
        }
    }
}
