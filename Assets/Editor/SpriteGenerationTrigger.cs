using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Gatillo de inicialización automática en Unity Editor:
/// Si la escena MainMenu.unity aún no ha sido creada o se importan los archivos,
/// extrae automáticamente el Arte HD de las capturas del usuario y monta el Menú Principal.
/// </summary>
[InitializeOnLoad]
public static class SpriteGenerationTrigger
{
    private const string MarkerPath = "Library/KiwiHDArtApplied.marker";

    static SpriteGenerationTrigger()
    {
        EditorApplication.delayCall += OnEditorReady;
    }

    private static void OnEditorReady()
    {
        // Si no existe el marcador de extracción o no existe MainMenu.unity
        if (!File.Exists(MarkerPath) || !File.Exists("Assets/Scenes/MainMenu.unity"))
        {
            Debug.Log("🎨 [Kiwi Auto-Init] Extrayendo Arte HD de las capturas y preparando Menú Principal...");
            
            // 1. Extraer Arte HD
            UserArtAssetExtractor.ExtractAndApplyAllArt(false);

            // 2. Montar Menú Principal si no existe
            if (!File.Exists("Assets/Scenes/MainMenu.unity"))
            {
                WorldBuilder.SetupMainMenu(false);
            }

            // 3. Crear marcador para no repetir cada frame
            try
            {
                File.WriteAllText(MarkerPath, System.DateTime.UtcNow.ToString());
            }
            catch { }

            Debug.Log("✨ [Kiwi Auto-Init] ¡Todo configurado correctamente con el Arte HD del usuario!");
        }
    }
}
