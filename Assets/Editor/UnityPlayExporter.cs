using UnityEditor;
using UnityEngine;
using System.IO;
using System.IO.Compression;

public class UnityPlayExporter
{
    [MenuItem("Kiwi/🌐 Generar ZIP WebGL para Unity Play", priority = 1)]
    public static void ExportWebGLZip()
    {
        // 1. Registrar todas las escenas del juego
        string[] scenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/ParamoToxico.unity",
            "Assets/Scenes/MarDePlastico.unity",
            "Assets/Scenes/CiudadDeHumo.unity",
            "Assets/Scenes/DesiertoDeResiduos.unity",
            "Assets/Scenes/BosqueMarchito.unity"
        };

        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            buildScenes[i] = new EditorBuildSettingsScene(scenes[i], true);
        }
        EditorBuildSettings.scenes = buildScenes;

        // 2. Ruta de salida temporal
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string buildFolder = Path.Combine(projectRoot, "Builds", "WebGL");

        if (Directory.Exists(buildFolder))
        {
            try { Directory.Delete(buildFolder, true); } catch {}
        }
        Directory.CreateDirectory(buildFolder);

        Debug.Log("🚀 Iniciando compilación WebGL para Unity Play...");
        EditorUtility.DisplayProgressBar("Compilando WebGL", "Construyendo escenas del juego para la web...", 0.3f);

        try
        {
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes = scenes;
            options.locationPathName = buildFolder;
            options.target = BuildTarget.WebGL;
            options.options = BuildOptions.None;

            var report = BuildPipeline.BuildPlayer(options);
            EditorUtility.ClearProgressBar();

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                string downloadsPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
                string zipPath = Path.Combine(downloadsPath, "Kiwi_WebGL_UnityPlay.zip");

                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(buildFolder, zipPath);

                EditorUtility.DisplayDialog(
                    "🎉 ¡Build WebGL Lista para Unity Play!",
                    $"El juego se compiló y se comprimió en tu carpeta Descargas:\n\n📁 {zipPath}\n\n¡Solo arrastra este archivo ZIP a la página de Unity Play que ya tienes abierta en tu navegador!",
                    "¡Entendido!"
                );
                EditorUtility.RevealInFinder(zipPath);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "⚠️ Módulo WebGL Requerido",
                    "Para compilar a WebGL, necesitas tener instalado el módulo 'WebGL Build Support' en Unity Hub:\n\n1. Abre Unity Hub.\n2. Ve a 'Installs'.\n3. Haz clic en la tuerca ⚙️ de Unity 6.\n4. Selecciona 'Add Modules' y marca 'WebGL Build Support'.\n5. Luego vuelve aquí y pulsa este botón de nuevo.",
                    "Aceptar"
                );
            }
        }
        catch (System.Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("Error al compilar WebGL: " + ex.Message);
            EditorUtility.DisplayDialog("Aviso", "Asegúrate de tener instalado el módulo WebGL en Unity Hub.\n\nError: " + ex.Message, "Cerrar");
        }
    }
}
