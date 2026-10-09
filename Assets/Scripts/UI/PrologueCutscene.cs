using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Cinemática de Prólogo Narrativo de Kiwi: El Guardián de la Última Semilla.
/// Presenta la historia ilustrada de la caída del mundo, la corrupción de los espíritus
/// y el destino de Kiwi como portador de la Última Semilla.
/// </summary>
public class PrologueCutscene : MonoBehaviour
{
    private struct StorySlide
    {
        public string actTitle;
        public string storyText;
        public string slideSpriteName;
    }

    private StorySlide[] slides = new StorySlide[]
    {
        new StorySlide
        {
            actTitle = "ACTO I: LA ARMONÍA ANCESTRAL",
            storyText = "En tiempos remotos, el mundo florecía en perfecto equilibrio.\n\nLos cuatro Espíritus Sagrados — Agua, Fuego, Aire y Tierra — alimentaban las raíces ancestrales y los océanos con su energía elemental pura.",
            slideSpriteName = "Assets/Sprites/PrologueSlide1.png"
        },
        new StorySlide
        {
            actTitle = "ACTO II: LA GRAN CONTAMINACIÓN",
            storyText = "Pero el avance insaciable de las fábricas y el desecho descontrolado cubrieron la tierra de plástico asfixiante, humos venenosos y ácidos corrosivos.\n\nLos cuatro espíritus fueron apresados y corrompidos en monstruosos cascarones de basura.",
            slideSpriteName = "Assets/Sprites/PrologueSlide2.png"
        },
        new StorySlide
        {
            actTitle = "ACTO III: LA ÚLTIMA SEMILLA",
            storyText = "Cuando el último árbol parecía haber muerto, la esencia viva del planeta se concentró en un fruto dorado resplandeciente: «La Última Semilla».\n\nSu brote sagrado posee el poder de disolver los residuos y sanar el mundo si alcanza el Santuario Natural.",
            slideSpriteName = "Assets/Sprites/PrologueSlide3.png"
        },
        new StorySlide
        {
            actTitle = "ACTO IV: EL DESPERTAR DEL GUARDIÁN",
            storyText = "Kiwi, un ágil gato de corazón noble, fue elegido para guardar la Semilla en su mochila.\n\nArmado con dagas elementales y el legendario Sobre-Salto, Kiwi desciende al Mar de Plástico para liberar al Espíritu del Agua e iniciar la gran purificación.",
            slideSpriteName = "Assets/Sprites/PrologueSlide4.png"
        }
    };

    private int currentSlideIndex = 0;
    private bool isCutsceneActive = false;
    private Action onCompleteCallback;

    private Texture2D[] slideTextures;
    private string displayedText = "";
    private bool isTyping = false;
    private Coroutine typingRoutine;

    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle promptStyle;
    private GUIStyle boxStyle;

    public void StartCutscene(Action onComplete)
    {
        onCompleteCallback = onComplete;
        isCutsceneActive = true;
        currentSlideIndex = 0;

        LoadSlideTextures();
        ShowCurrentSlide();
    }

    private void LoadSlideTextures()
    {
        slideTextures = new Texture2D[slides.Length];
        for (int i = 0; i < slides.Length; i++)
        {
            #if UNITY_EDITOR
            slideTextures[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(slides[i].slideSpriteName);
            #endif
        }
    }

    private void ShowCurrentSlide()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypewriterRoutine(slides[currentSlideIndex].storyText));
    }

    private IEnumerator TypewriterRoutine(string fullText)
    {
        isTyping = true;
        displayedText = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            displayedText += fullText[i];
            // Sonido sutil cada pocos caracteres
            if (i % 4 == 0 && fullText[i] != ' ' && fullText[i] != '\n')
            {
                KiwiAudio.PlayEnemyHit(); // Pequeño blip tipográfico
            }
            yield return new WaitForSeconds(0.022f);
        }

        isTyping = false;
    }

    private void Update()
    {
        if (!isCutsceneActive) return;

        // Saltar todo el prólogo
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FinishCutscene();
            return;
        }

        // Avanzar slide
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // Completar texto instantáneamente
                if (typingRoutine != null) StopCoroutine(typingRoutine);
                displayedText = slides[currentSlideIndex].storyText;
                isTyping = false;
            }
            else
            {
                // Pasar al siguiente acto
                currentSlideIndex++;
                KiwiAudio.PlayJump();

                if (currentSlideIndex >= slides.Length)
                {
                    FinishCutscene();
                }
                else
                {
                    ShowCurrentSlide();
                }
            }
        }
    }

    private void FinishCutscene()
    {
        isCutsceneActive = false;
        KiwiAudio.PlayDoubleJump();
        onCompleteCallback?.Invoke();
    }

    private void OnGUI()
    {
        if (!isCutsceneActive) return;

        InitStyles();

        // 1. Fondo Oscuro Cinematográfico
        GUI.color = new Color(0.04f, 0.05f, 0.08f, 1f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // 2. Imagen Ilustrada del Acto (Postal panorámica en el tercio superior)
        if (slideTextures != null && currentSlideIndex < slideTextures.Length && slideTextures[currentSlideIndex] != null)
        {
            float imgW = Mathf.Min(640, Screen.width - 60);
            float imgH = imgW * (90f / 160f);
            float imgX = (Screen.width - imgW) / 2f;
            float imgY = 30;

            // Marco sutil
            GUI.color = new Color(0.2f, 0.8f, 1f, 0.5f);
            GUI.Box(new Rect(imgX - 4, imgY - 4, imgW + 8, imgH + 8), GUIContent.none);
            GUI.color = Color.white;

            GUI.DrawTexture(new Rect(imgX, imgY, imgW, imgH), slideTextures[currentSlideIndex], ScaleMode.StretchToFill);
        }

        // 3. Cuadro de Diálogo Narrativo (Parte inferior)
        float boxW = Mathf.Min(780, Screen.width - 40);
        float boxH = 210;
        float boxX = (Screen.width - boxW) / 2f;
        float boxY = Screen.height - boxH - 30;

        GUI.color = new Color(0.08f, 0.1f, 0.16f, 0.95f);
        GUI.Box(new Rect(boxX, boxY, boxW, boxH), GUIContent.none);
        GUI.color = Color.white;

        // Título del Acto
        GUI.Label(new Rect(boxX + 24, boxY + 16, boxW - 48, 28), slides[currentSlideIndex].actTitle, titleStyle);

        // Texto narrado
        GUI.Label(new Rect(boxX + 28, boxY + 52, boxW - 56, boxH - 95), displayedText, bodyStyle);

        // Indicador de avance
        string prompt = isTyping ? "▶ Haz clic o pulsa [Espacio] para completar el texto" : $"▶ Pulsa [Espacio / E] para continuar ({currentSlideIndex + 1}/{slides.Length})  •  [ESC] Saltar Prólogo";
        GUI.Label(new Rect(boxX + 24, boxY + boxH - 32, boxW - 48, 24), prompt, promptStyle);
    }

    private void InitStyles()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
        }

        if (bodyStyle == null)
        {
            bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.fontSize = 15;
            bodyStyle.wordWrap = true;
            bodyStyle.alignment = TextAnchor.UpperLeft;
            bodyStyle.normal.textColor = new Color(0.92f, 0.95f, 0.98f);
        }

        if (promptStyle == null)
        {
            promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 12;
            promptStyle.fontStyle = FontStyle.Italic;
            promptStyle.alignment = TextAnchor.MiddleRight;
            promptStyle.normal.textColor = new Color(0.3f, 0.85f, 1f, 0.75f);
        }
    }
}
