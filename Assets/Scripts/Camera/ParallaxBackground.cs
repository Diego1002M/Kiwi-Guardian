using UnityEngine;

/// <summary>
/// Mueve el fondo marino suavemente siguiendo la cámara para dar sensación de profundidad y amplitud de océano.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private float parallaxEffect = 0.85f; // Factor de seguimiento
    private Transform camTransform;
    private Vector3 lastCamPos;

    private void Start()
    {
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
            lastCamPos = camTransform.position;
        }
    }

    private void LateUpdate()
    {
        if (camTransform == null)
        {
            if (Camera.main != null) camTransform = Camera.main.transform;
            return;
        }

        Vector3 deltaMovement = camTransform.position - lastCamPos;
        transform.position += new Vector3(deltaMovement.x * parallaxEffect, deltaMovement.y * (parallaxEffect * 0.5f), 0);
        lastCamPos = camTransform.position;
    }
}
