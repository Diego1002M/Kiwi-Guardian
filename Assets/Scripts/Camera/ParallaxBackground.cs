using UnityEngine;

/// <summary>
/// Controla el fondo de la escena para que persiga a Kiwi y a la cámara en todo momento.
/// Se adapta automáticamente a la pantalla en modo Simple (una sola imagen nítida que nunca se duplica ni se corta).
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Seguimiento del Personaje / Cámara")]
    [Tooltip("1 = Sigue 100% a la cámara (el fondo acompaña siempre al jugador sin salirse ni duplicarse).")]
    [Range(0f, 1f)]
    [SerializeField] private float followSpeedX = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] private float followSpeedY = 0.85f;

    [Header("Ajuste de Pantalla")]
    [SerializeField] private bool autoFitScreen = true;
    [SerializeField] private float sizeMultiplier = 1.4f; // Margen para que nunca se vean bordes
    [SerializeField] private float depthZ = 12f; // Posición Z profunda detrás del escenario

    private Camera targetCamera;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialCamPos;
    private Vector3 initialBgPos;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Forzar modo Simple: una sola imagen panorámica limpia, sin repetición en cuadrícula
            spriteRenderer.drawMode = SpriteDrawMode.Simple;
            spriteRenderer.sortingOrder = -100;
        }
    }

    private void Start()
    {
        FindCam();
        if (targetCamera != null)
        {
            initialCamPos = targetCamera.transform.position;
            initialBgPos = transform.position;
            FitToCamera();
        }
    }

    private void FindCam()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    public void FitToCamera()
    {
        FindCam();
        if (targetCamera == null || spriteRenderer == null || spriteRenderer.sprite == null || !autoFitScreen) return;

        float camHeight = targetCamera.orthographicSize * 2f;
        float camWidth = camHeight * targetCamera.aspect;

        float spriteW = spriteRenderer.sprite.rect.width / spriteRenderer.sprite.pixelsPerUnit;
        float spriteH = spriteRenderer.sprite.rect.height / spriteRenderer.sprite.pixelsPerUnit;

        if (spriteW > 0 && spriteH > 0)
        {
            float scaleX = (camWidth / spriteW) * sizeMultiplier;
            float scaleY = (camHeight / spriteH) * sizeMultiplier;
            float finalScale = Mathf.Max(scaleX, scaleY);
            transform.localScale = new Vector3(finalScale, finalScale, 1f);
        }
    }

    private void LateUpdate()
    {
        FindCam();
        if (targetCamera == null) return;

        Vector3 camPos = targetCamera.transform.position;
        Vector3 delta = camPos - initialCamPos;

        // El fondo persigue la posición de la cámara / Kiwi para que siempre esté visible y centrado
        float newX = initialBgPos.x + delta.x * followSpeedX;
        float newY = initialBgPos.y + delta.y * followSpeedY;

        transform.position = new Vector3(newX, newY, depthZ);
    }
}
