using UnityEngine;

/// <summary>
/// Controla el fondo de la escena para que persiga a Kiwi y a la cámara en todo momento.
/// Se emparenta dinámicamente a la cámara principal para un seguimiento 100% perfecto,
/// suave y sin desfases, cubriendo toda la pantalla sin duplicarse ni cortarse jamás.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Ajuste y Perspectiva")]
    [Tooltip("Distancia Z detrás de la acción del juego.")]
    [SerializeField] private float depthZ = 12f;

    [Tooltip("Desplazamiento vertical opcional.")]
    [SerializeField] private float yOffset = 0f;

    [Tooltip("Multiplicador de tamaño para garantizar que no queden bordes negros.")]
    [SerializeField] private float sizeMultiplier = 1.35f;

    private Camera targetCamera;
    private SpriteRenderer spriteRenderer;
    private float lastAspect = -1f;
    private float lastOrthoSize = -1f;

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
        AttachToCamera();
    }

    private void OnEnable()
    {
        AttachToCamera();
    }

    private void AttachToCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            // Emparentar directamente a la cámara para seguimiento 1:1 instantáneo y sin temblores
            transform.SetParent(targetCamera.transform, false);
            transform.localPosition = new Vector3(0f, yOffset, depthZ);
            transform.localRotation = Quaternion.identity;
            FitToCamera();
        }
    }

    public void FitToCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        lastAspect = targetCamera.aspect;
        lastOrthoSize = targetCamera.orthographicSize;

        float camHeight = targetCamera.orthographicSize * 2f;
        float camWidth = camHeight * targetCamera.aspect;

        float spriteW = spriteRenderer.sprite.rect.width / spriteRenderer.sprite.pixelsPerUnit;
        float spriteH = spriteRenderer.sprite.rect.height / spriteRenderer.sprite.pixelsPerUnit;

        if (spriteW > 0.001f && spriteH > 0.001f)
        {
            float scaleX = (camWidth / spriteW) * sizeMultiplier;
            float scaleY = (camHeight / spriteH) * sizeMultiplier;
            float finalScale = Mathf.Max(scaleX, scaleY);
            transform.localScale = new Vector3(finalScale, finalScale, 1f);
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            AttachToCamera();
            return;
        }

        // Si por alguna razón se desvinculó de la cámara, re-emparentar o seguir posición
        if (transform.parent != targetCamera.transform)
        {
            transform.SetParent(targetCamera.transform, false);
            transform.localPosition = new Vector3(0f, yOffset, depthZ);
        }
        else
        {
            // Mantener centrado exacto
            if (transform.localPosition.x != 0f || transform.localPosition.y != yOffset || transform.localPosition.z != depthZ)
            {
                transform.localPosition = new Vector3(0f, yOffset, depthZ);
            }
        }

        // Reajustar tamaño si la resolución o aspect ratio cambia en tiempo real
        if (Mathf.Abs(targetCamera.aspect - lastAspect) > 0.01f || Mathf.Abs(targetCamera.orthographicSize - lastOrthoSize) > 0.01f)
        {
            FitToCamera();
        }
    }
}
