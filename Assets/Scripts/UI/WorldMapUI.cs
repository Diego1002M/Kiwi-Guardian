using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pantalla de Mapa Mundial estilo nodos conectados (Metroidvania / Hollow Knight).
/// Se abre con la tecla [M]. Muestra el Páramo Tóxico en el centro y las 4 regiones alrededor.
/// </summary>
public class WorldMapUI : MonoBehaviour
{
    private bool isMapOpen = false;

    private void Update()
    {
        // Abrir/cerrar mapa con la tecla M o TAB
        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Tab))
        {
            isMapOpen = !isMapOpen;
        }

        if (isMapOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            isMapOpen = false;
        }
    }

    private void OnGUI()
    {
        if (!isMapOpen)
        {
            // Pequeño recordatorio en la esquina superior derecha
            GUIStyle mapHintStyle = new GUIStyle(GUI.skin.label);
            mapHintStyle.fontSize = 12;
            mapHintStyle.fontStyle = FontStyle.Bold;
            mapHintStyle.normal.textColor = new Color(0.2f, 0.9f, 1f, 0.8f);
            GUI.Label(new Rect(Screen.width - 150, 18, 140, 24), "🗺️ Mapa: Pulsa [M]", mapHintStyle);
            return;
        }

        // 1. Fondo oscurecido semitransparente estilo Hollow Knight
        GUI.color = new Color(0.04f, 0.07f, 0.12f, 0.96f);
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);
        GUI.color = Color.white;

        // Marco del Mapa
        float mapW = Mathf.Min(780, Screen.width - 40);
        float mapH = Mathf.Min(520, Screen.height - 40);
        float startX = (Screen.width - mapW) / 2f;
        float startY = (Screen.height - mapH) / 2f;

        GUI.color = new Color(0.1f, 0.16f, 0.24f, 0.95f);
        GUI.Box(new Rect(startX, startY, mapW, mapH), GUIContent.none);
        GUI.color = Color.white;

        // Título del Mapa
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 20;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.2f, 0.95f, 0.85f);
        GUI.Label(new Rect(startX, startY + 15, mapW, 30), "🗺️ MAPA DEL MUNDO — LA ÚLTIMA SEMILLA", titleStyle);

        string currentScene = SceneManager.GetActiveScene().name;

        // Coordenadas de los nodos
        Vector2 centerNode = new Vector2(startX + mapW / 2f, startY + mapH / 2f + 15);
        Vector2 westNode = new Vector2(centerNode.x - 220, centerNode.y); // Mar de Plástico
        Vector2 northNode = new Vector2(centerNode.x, centerNode.y - 140); // Ciudad de Humo
        Vector2 eastNode = new Vector2(centerNode.x + 220, centerNode.y); // Bosque Marchito
        Vector2 southNode = new Vector2(centerNode.x, centerNode.y + 130); // Desierto de Residuos
        Vector2 sanctuaryNode = new Vector2(centerNode.x, centerNode.y - 195); // Santuario Natural

        // Dibujar líneas conectoras
        DrawConnection(centerNode, westNode);
        DrawConnection(centerNode, northNode);
        DrawConnection(centerNode, eastNode);
        DrawConnection(centerNode, southNode);
        DrawConnection(northNode, sanctuaryNode);

        // Dibujar Nodos
        DrawNode("☠️ Páramo Tóxico\n(Hub Central)", centerNode, currentScene == "ParamoToxico", new Color(0.6f, 0.5f, 0.7f), "ParamoToxico");
        DrawNode("🌊 Mar de Plástico\n(Agua)", westNode, currentScene == "MarDePlastico", new Color(0.2f, 0.8f, 1f), "MarDePlastico");
        DrawNode("💨 Ciudad de Humo\n(Aire)", northNode, currentScene == "CiudadDeHumo", new Color(0.8f, 0.85f, 0.9f), "CiudadDeHumo");
        DrawNode("🌱 Bosque Marchito\n(Tierra)", eastNode, currentScene == "BosqueMarchito", new Color(0.4f, 0.85f, 0.4f), "BosqueMarchito");
        DrawNode("🔥 Desierto de Residuos\n(Fuego)", southNode, currentScene == "DesiertoDeResiduos", new Color(1f, 0.6f, 0.2f), "DesiertoDeResiduos");
        DrawNode("🌿 Santuario Natural\n(Destino Final)", sanctuaryNode, false, new Color(0.2f, 1f, 0.6f), null);

        // Pie del mapa
        GUIStyle footerStyle = new GUIStyle(GUI.skin.label);
        footerStyle.fontSize = 13;
        footerStyle.alignment = TextAnchor.MiddleCenter;
        footerStyle.normal.textColor = Color.gray;
        GUI.Label(new Rect(startX, startY + mapH - 35, mapW, 25), "Haz clic en una región explorada para viajar (si estás en el Hub)  |  Presiona [M] para cerrar", footerStyle);
    }

    private void DrawConnection(Vector2 from, Vector2 to)
    {
        Vector2 diff = to - from;
        float dist = diff.magnitude;
        int steps = Mathf.Max(1, (int)(dist / 14));

        GUI.color = new Color(0.3f, 0.45f, 0.6f, 0.6f);
        for (int i = 0; i <= steps; i++)
        {
            Vector2 p = Vector2.Lerp(from, to, (float)i / steps);
            GUI.Box(new Rect(p.x - 2, p.y - 2, 4, 4), GUIContent.none);
        }
        GUI.color = Color.white;
    }

    private void DrawNode(string label, Vector2 pos, bool isCurrent, Color themeColor, string sceneToLoad)
    {
        float w = 150;
        float h = 55;
        Rect r = new Rect(pos.x - w / 2f, pos.y - h / 2f, w, h);

        // Fondo del nodo
        GUI.color = isCurrent ? new Color(0.2f, 0.4f, 0.6f, 0.95f) : new Color(0.08f, 0.12f, 0.18f, 0.9f);
        GUI.Box(r, GUIContent.none);

        // Borde resaltado
        GUI.color = isCurrent ? Color.yellow : themeColor;
        GUI.Box(new Rect(r.x, r.y, r.width, 2), GUIContent.none);
        GUI.Box(new Rect(r.x, r.y + r.height - 2, r.width, 2), GUIContent.none);

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 11;
        style.fontStyle = isCurrent ? FontStyle.Bold : FontStyle.Normal;
        style.normal.textColor = isCurrent ? Color.yellow : Color.white;

        string display = label;
        if (isCurrent) display = "📍 ESTÁS AQUÍ\n" + label;

        GUI.Label(r, display, style);

        // Si se hace clic en el nodo y hay una escena asignada
        if (!string.IsNullOrEmpty(sceneToLoad) && GUI.Button(r, GUIContent.none, GUIStyle.none))
        {
            if (SceneManager.GetActiveScene().name == "ParamoToxico" || isCurrent)
            {
                if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
                {
                    isMapOpen = false;
                    SceneManager.LoadScene(sceneToLoad);
                }
            }
        }
        GUI.color = Color.white;
    }
}
