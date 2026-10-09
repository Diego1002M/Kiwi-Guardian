using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de retroalimentación de combate visual ("Juice"):
/// - Textos emergentes flotantes (Popups de daño crítico, reacciones elementales).
/// - Parpadeo blanco de impacto (Hit Flash).
/// </summary>
public class CombatFeedback : MonoBehaviour
    {
        private static CombatFeedback instance;

        private class Popup
        {
            public Vector3 worldPos;
            public string text;
            public Color color;
            public float timer;
            public float duration;
            public float scale;
        }

        private List<Popup> activePopups = new List<Popup>();
        private Camera mainCam;
        private GUIStyle popupStyle;

        private void Awake()
        {
            if (instance == null) instance = this;
            mainCam = Camera.main;
        }

        public static void ShowPopup(Vector3 worldPos, string text, Color color, float duration = 0.8f)
        {
            if (instance == null)
            {
                GameObject go = new GameObject("--- COMBAT FEEDBACK ---");
                instance = go.AddComponent<CombatFeedback>();
            }

            Popup p = new Popup
            {
                worldPos = worldPos + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.2f, 0.6f), 0f),
                text = text,
                color = color,
                timer = 0f,
                duration = duration,
                scale = 1.4f
            };
            instance.activePopups.Add(p);
        }

        public static void Flash(MonoBehaviour caller, SpriteRenderer sr, Color flashColor, float duration = 0.08f)
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
                Popup p = activePopups[i];
                p.timer += Time.deltaTime;
                p.worldPos += Vector3.up * 1.5f * Time.deltaTime;
                p.scale = Mathf.Lerp(p.scale, 1.0f, Time.deltaTime * 8f);

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
                Popup p = activePopups[i];
                Vector3 screenPos = mainCam.WorldToScreenPoint(p.worldPos);
                if (screenPos.z > 0)
                {
                    float alpha = 1f - (p.timer / p.duration);
                    Color c = p.color;
                    c.a = alpha;

                    int fontSize = Mathf.RoundToInt(15 * p.scale);
                    popupStyle.fontSize = Mathf.Clamp(fontSize, 11, 24);

                    // Sombra oscura detrás del texto para alta legibilidad
                    popupStyle.normal.textColor = new Color(0.05f, 0.05f, 0.08f, alpha * 0.9f);
                    GUI.Label(new Rect(screenPos.x - 79, Screen.height - screenPos.y - 14, 160, 30), p.text, popupStyle);
                    GUI.Label(new Rect(screenPos.x - 81, Screen.height - screenPos.y - 16, 160, 30), p.text, popupStyle);

                    // Texto principal en color brillante
                    popupStyle.normal.textColor = c;
                    GUI.Label(new Rect(screenPos.x - 80, Screen.height - screenPos.y - 15, 160, 30), p.text, popupStyle);
                }
            }
        }
    }
