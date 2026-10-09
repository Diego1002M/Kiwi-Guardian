using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Portal de viaje entre regiones del mundo de Kiwi (Metroidvania).
/// Permite viajar entre el Páramo Tóxico (Hub) y las diferentes regiones elementales.
/// </summary>
public class RegionPortal : MonoBehaviour
{
    [Header("Destino")]
    [Tooltip("Nombre de la escena a cargar")]
    [SerializeField] private string targetSceneName = "ParamoToxico";
    [Tooltip("Nombre legible para el jugador")]
    [SerializeField] private string destinationDisplayName = "Páramo Tóxico (Hub Central)";

    [Header("Efectos")]
    [SerializeField] private Color portalColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private float pulseSpeed = 3.5f;

    private Transform playerTransform;
    private SpriteRenderer sr;
    private bool isPlayerInside = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        // Efecto de pulso místico
        if (sr != null)
        {
            float alpha = 0.7f + Mathf.Sin(Time.time * pulseSpeed) * 0.25f;
            Color c = portalColor;
            c.a = alpha;
            sr.color = c;
        }

        // Detectar si el jugador presiona [E] dentro del portal
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            Teleport();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    public void Teleport()
    {
        KiwiAudio.PlayDoubleJump();
        Debug.Log($"🌀 Viajando a: {destinationDisplayName} ({targetSceneName})");

        // Cargar escena si está en Build Settings o disponible
        if (Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning($"La escena '{targetSceneName}' aún no está registrada en Build Settings. Intenta abrirla desde Assets/Scenes/{targetSceneName}.unity.");
        }
    }

    public void SetDestination(string sceneName, string displayName, Color color)
    {
        targetSceneName = sceneName;
        destinationDisplayName = displayName;
        portalColor = color;
        if (sr != null) sr.color = color;
    }

    private void OnGUI()
    {
        if (isPlayerInside)
        {
            Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.6f) : Vector3.zero;
            if (screenPos.z > 0)
            {
                float w = 260;
                float h = 48;
                Rect rect = new Rect(screenPos.x - w / 2f, Screen.height - screenPos.y - h / 2f, w, h);

                GUI.color = new Color(0.06f, 0.1f, 0.16f, 0.92f);
                GUI.Box(rect, GUIContent.none);
                GUI.color = Color.white;

                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 12;
                style.normal.textColor = portalColor;

                GUI.Label(rect, $"🌀 Presiona [E] para viajar:\n{destinationDisplayName}", style);
            }
        }
    }
}
