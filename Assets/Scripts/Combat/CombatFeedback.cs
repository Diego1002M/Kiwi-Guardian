using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema avanzado de "Game Feel" y retroalimentación de combate visual ("Juice"):
/// - Números de daño flotantes animados con rebote y colores según el tipo de impacto (Normal, Crítico, Jugador, Cura).
/// - Destellos de impacto (Hit Flash) en enemigos y jefes para sentir cada golpe.
/// - Sacudida de pantalla sutil en impactos fuertes y golpes críticos.
/// </summary>
public class CombatFeedback : MonoBehaviour
{
    private static CombatFeedback instance;

    public static CombatFeedback Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("--- COMBAT FEEDBACK ---");
                instance = go.AddComponent<CombatFeedback>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private class DamagePopup
    {
        public Vector3 worldPos;
        public string text;
        public Color color;
        public float timer;
        public float duration;
        public float initialScale;
        public float currentScale;
        public Vector2 velocity;
    }

    private List<DamagePopup> activePopups = new List<DamagePopup>();
    private Camera mainCam;
    private GUIStyle popupStyle;

    private void Awake()
    {
        // Si este script está en una cámara o un objeto de escena, NUNCA volver inmortal todo el GameObject
        // Solo el objeto raíz dedicado "--- COMBAT FEEDBACK ---" debe ser DontDestroyOnLoad
        if (GetComponent<Camera>() != null || transform.parent != null || gameObject.name != "--- COMBAT FEEDBACK ---")
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }
            instance = this;
            mainCam = GetComponent<Camera>() ?? Camera.main;
            return;
        }

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        mainCam = Camera.main;
    }

    /// <summary>
    /// Muestra un número de daño flotante con rebote y estilo visual.
    /// </summary>
    public static void ShowDamage(Vector3 worldPos, int amount, bool isCritical = false)
    {
        string txt = isCritical ? $"¡CRÍTICO! -{amount}" : $"-{amount}";
        Color col = isCritical ? new Color(1f, 0.85f, 0.1f) : new Color(1f, 0.95f, 0.75f);
        float scale = isCritical ? 1.6f : 1.25f;
        float dur = isCritical ? 1.0f : 0.75f;

        ShowPopupCustom(worldPos, txt, col, dur, scale);

        if (isCritical)
        {
            Platformer2D.CameraFollow2D.Shake(0.18f, 0.22f);
        }
    }

    /// <summary>
    /// Muestra aviso de daño recibido por Kiwi (rojo brillante).
    /// </summary>
    public static void ShowPlayerHurt(Vector3 worldPos, int damage = 1)
    {
        ShowPopupCustom(worldPos, $"-{damage} ♥", new Color(1f, 0.25f, 0.25f), 0.9f, 1.4f);
        Platformer2D.CameraFollow2D.Shake(0.2f, 0.25f);
    }

    /// <summary>
    /// Muestra aviso de curación o recolección positiva (verde esmeralda o dorado).
    /// </summary>
    public static void ShowPopup(Vector3 worldPos, string text, Color color, float duration = 0.8f)
    {
        ShowPopupCustom(worldPos, text, color, duration, 1.3f);
    }

    public static void ShowPopupCustom(Vector3 worldPos, string text, Color color, float duration, float scale)
    {
        Instance.SpawnPopupInternal(worldPos, text, color, duration, scale);
    }

    private void SpawnPopupInternal(Vector3 worldPos, string text, Color color, float duration, float scale)
    {
        // Ligero desplazamiento aleatorio para que los números consecutivos no se tapen
        Vector3 spawnPos = worldPos + new Vector3(Random.Range(-0.35f, 0.35f), Random.Range(0.4f, 0.85f), 0f);
        Vector2 vel = new Vector2(Random.Range(-0.8f, 0.8f), Random.Range(2.2f, 3.2f));

        DamagePopup p = new DamagePopup
        {
            worldPos = spawnPos,
            text = text,
            color = color,
            timer = 0f,
            duration = duration,
            initialScale = scale,
            currentScale = scale,
            velocity = vel
        };

        activePopups.Add(p);
    }

    /// <summary>
    /// Congela momentáneamente el tiempo (Hit-Stop / Freeze Frame) en impactos contundentes estilo Hollow Knight / Smash Bros.
    /// </summary>
    public static void TriggerHitStop(float duration = 0.055f)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.HitStopRoutine(duration));
        }
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        float prevTimeScale = Time.timeScale;
        Time.timeScale = 0.02f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = (prevTimeScale > 0.05f) ? prevTimeScale : 1.0f;
    }

    /// <summary>
    /// Hace que el SpriteRenderer parpadee en blanco brillante o color de impacto.
    /// </summary>
    public static void Flash(MonoBehaviour caller, SpriteRenderer sr, Color flashColor, float duration = 0.09f)
    {
        if (caller != null && caller.isActiveAndEnabled && sr != null)
        {
            caller.StartCoroutine(FlashRoutine(sr, flashColor, duration));
        }
    }

    private static IEnumerator FlashRoutine(SpriteRenderer sr, Color flashColor, float duration)
    {
        if (sr == null) yield break;
        Color orig = sr.color;
        sr.color = flashColor;
        yield return new WaitForSeconds(duration);
        if (sr != null) sr.color = orig;
    }

    private void Update()
    {
        if (mainCam == null) mainCam = Camera.main;

        for (int i = activePopups.Count - 1; i >= 0; i--)
        {
            DamagePopup p = activePopups[i];
            p.timer += Time.deltaTime;

            // Movimiento físico con desaceleración y gravedad suave
            p.worldPos += (Vector3)p.velocity * Time.deltaTime;
            p.velocity.y -= 3.5f * Time.deltaTime;
            p.velocity.x = Mathf.Lerp(p.velocity.x, 0f, Time.deltaTime * 3f);

            // Escala animada (aparece grande y se asienta)
            p.currentScale = Mathf.Lerp(p.currentScale, 1.0f, Time.deltaTime * 7f);

            if (p.timer >= p.duration)
            {
                activePopups.RemoveAt(i);
            }
        }
    }

    private void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;
        if (mainCam == null || activePopups.Count == 0) return;

        if (popupStyle == null)
        {
            popupStyle = new GUIStyle(GUI.skin.label);
            popupStyle.alignment = TextAnchor.MiddleCenter;
            popupStyle.fontStyle = FontStyle.Bold;
        }

        for (int i = 0; i < activePopups.Count; i++)
        {
            DamagePopup p = activePopups[i];
            Vector3 screenPos = mainCam.WorldToScreenPoint(p.worldPos);

            // Solo renderizar si está dentro de la vista de la cámara
            if (screenPos.z > 0 && screenPos.x >= -100 && screenPos.x <= Screen.width + 100)
            {
                float t = p.timer / p.duration;
                // Fade out en el último tercio de duración
                float alpha = t > 0.65f ? Mathf.Lerp(1f, 0f, (t - 0.65f) / 0.35f) : 1f;

                Color c = p.color;
                c.a = alpha;

                int fontSize = Mathf.RoundToInt(15 * p.currentScale);
                popupStyle.fontSize = Mathf.Clamp(fontSize, 11, 26);

                float width = 220;
                float height = 36;
                Rect labelRect = new Rect(screenPos.x - width / 2f, Screen.height - screenPos.y - height / 2f, width, height);

                // Sombra oscura detrás del texto para máxima nitidez pixel art
                popupStyle.normal.textColor = new Color(0.04f, 0.04f, 0.06f, alpha * 0.95f);
                GUI.Label(new Rect(labelRect.x + 1.5f, labelRect.y + 1.5f, width, height), p.text, popupStyle);
                GUI.Label(new Rect(labelRect.x - 1.5f, labelRect.y - 1.5f, width, height), p.text, popupStyle);

                // Texto principal iluminado
                popupStyle.normal.textColor = c;
                GUI.Label(labelRect, p.text, popupStyle);
            }
        }
    }
}
