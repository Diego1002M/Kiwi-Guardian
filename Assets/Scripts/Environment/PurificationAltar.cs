using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Altar de Purificación Elemental y Gran Altar del Árbol de la Vida.
/// Permite purificar regiones individuales o activar la Gran Victoria del juego en el Páramo Tóxico.
/// </summary>
public class PurificationAltar : MonoBehaviour
{
    [Header("Configuración de Región")]
    [SerializeField] private string regionKey = "Water"; // Water, Forest, Air, Earth, GrandAltar
    [SerializeField] private string regionDisplayName = "Mar de Plástico";
    [SerializeField] private bool isGrandAltar = false;

    [Header("Efectos Visuales")]
    [SerializeField] private Color purifiedSkyColor = new Color(0.12f, 0.45f, 0.55f);
    [SerializeField] private Sprite purifiedSprite;

    private bool isPurified = false;
    private bool showBanner = false;
    private float bannerTimer = 0f;
    private bool playerNearby = false;

    private void Awake()
    {
        if (purifiedSprite == null)
        {
            purifiedSprite = Resources.Load<Sprite>("Sprites/GiantSeed");
            if (purifiedSprite == null) purifiedSprite = Resources.Load<Sprite>("Sprites/RestTotem");
        }

        // Si ya fue purificado en sesiones previas, reflejarlo
        if (!isGrandAltar && PlayerPrefs.GetInt("Purified_" + regionKey, 0) == 1)
        {
            isPurified = true;
            ApplyPurifiedVisuals(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNearby = true;
            if (!isPurified)
            {
                Purify(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    public void Purify(GameObject player)
    {
        if (isGrandAltar)
        {
            CheckGrandVictory(player);
            return;
        }

        isPurified = true;
        PlayerPrefs.SetInt("Purified_" + regionKey, 1);
        PlayerPrefs.Save();

        // Restaurar salud de Kiwi al máximo
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null) health.Heal(health.MaxHealth);

        // Audio triunfal
        KiwiAudio.PlayVictory();
        CameraFollow2D.Shake(0.4f, 0.5f);

        ApplyPurifiedVisuals(true);
        StartCoroutine(ShowBannerRoutine());

        Debug.Log($"🌱 ¡REGIÓN PURIFICADA! {regionDisplayName} ha renacido gracias a la Última Semilla de Kiwi.");
    }

    private void CheckGrandVictory(GameObject player)
    {
        bool water = PlayerPrefs.GetInt("Boss_Water_Defeated", 0) == 1;
        bool forest = PlayerPrefs.GetInt("Boss_Forest_Defeated", 0) == 1;
        bool air = PlayerPrefs.GetInt("Boss_Air_Defeated", 0) == 1;
        bool earth = PlayerPrefs.GetInt("Boss_Earth_Defeated", 0) == 1;

        if (water && forest && air && earth)
        {
            isPurified = true;
            PlayerPrefs.SetInt("Game_Completed", 1);
            PlayerPrefs.Save();

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null) health.Heal(health.MaxHealth);

            KiwiAudio.PlayVictory();
            CameraFollow2D.Shake(0.8f, 0.8f);

            ApplyPurifiedVisuals(true);
            StartCoroutine(ShowBannerRoutine());
        }
        else
        {
            KiwiAudio.PlayDoubleJump();
        }
    }

    private void ApplyPurifiedVisuals(bool changeSky)
    {
        Camera cam = Camera.main;
        if (cam != null && changeSky)
        {
            cam.backgroundColor = purifiedSkyColor;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (purifiedSprite != null) sr.sprite = purifiedSprite;
            sr.color = Color.white;
        }
    }

    private IEnumerator ShowBannerRoutine()
    {
        showBanner = true;
        bannerTimer = 5f;
        while (bannerTimer > 0f)
        {
            bannerTimer -= Time.deltaTime;
            yield return null;
        }
        showBanner = false;
    }

    private void OnGUI()
    {
        // 1. Banner triunfal al purificar
        if (showBanner)
        {
            float w = Mathf.Min(680, Screen.width - 40);
            float h = isGrandAltar ? 130 : 90;
            Rect rect = new Rect((Screen.width - w) / 2f, 40, w, h);

            GUI.color = new Color(0.04f, 0.12f, 0.08f, 0.95f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = isGrandAltar ? 20 : 17;
            titleStyle.normal.textColor = new Color(0.3f, 1f, 0.5f);

            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.alignment = TextAnchor.MiddleCenter;
            subStyle.fontSize = 13;
            subStyle.normal.textColor = Color.white;

            if (isGrandAltar)
            {
                GUI.Label(new Rect(rect.x, rect.y + 12, rect.width, 32), "🌟 ¡EL MUNDO HA SIDO SALVADO! 🌟", titleStyle);
                GUI.Label(new Rect(rect.x, rect.y + 48, rect.width, 70), "Kiwi ha restaurado los 4 elementos primordiales.\nEl Árbol de la Vida florece nuevamente en la Tierra.", subStyle);
            }
            else
            {
                GUI.Label(new Rect(rect.x, rect.y + 12, rect.width, 30), $"✨ ¡{regionDisplayName.ToUpper()} PURIFICADO! ✨", titleStyle);
                GUI.Label(new Rect(rect.x, rect.y + 44, rect.width, 36), "La Semilla Ancestral ha purificado el ecosistema. Salud restaurada.", subStyle);
            }
        }

        // 2. Información del Gran Altar en el Hub Central
        if (isGrandAltar && playerNearby && !showBanner)
        {
            bool water = PlayerPrefs.GetInt("Boss_Water_Defeated", 0) == 1;
            bool forest = PlayerPrefs.GetInt("Boss_Forest_Defeated", 0) == 1;
            bool air = PlayerPrefs.GetInt("Boss_Air_Defeated", 0) == 1;
            bool earth = PlayerPrefs.GetInt("Boss_Earth_Defeated", 0) == 1;
            int count = (water ? 1 : 0) + (forest ? 1 : 0) + (air ? 1 : 0) + (earth ? 1 : 0);

            float w = 360;
            float h = 140;
            Rect rect = new Rect((Screen.width - w) / 2f, 60, w, h);

            GUI.color = new Color(0.05f, 0.08f, 0.15f, 0.92f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            GUIStyle styleTitle = new GUIStyle(GUI.skin.label);
            styleTitle.alignment = TextAnchor.MiddleCenter;
            styleTitle.fontStyle = FontStyle.Bold;
            styleTitle.fontSize = 14;
            styleTitle.normal.textColor = Color.cyan;

            GUIStyle styleLines = new GUIStyle(GUI.skin.label);
            styleLines.alignment = TextAnchor.MiddleCenter;
            styleLines.fontSize = 12;
            styleLines.normal.textColor = Color.white;

            GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, 24), $"🌱 GRAN ÁRBOL DE LA VIDA ({count}/4 Semillas)", styleTitle);

            string sW = water ? "<color=#4ef>💧 Agua: PURIFICADA</color>" : "<color=#888>💧 Agua: Corrompida</color>";
            string sF = forest ? "<color=#4f8>🌿 Bosque: PURIFICADO</color>" : "<color=#888>🌿 Bosque: Corrompido</color>";
            string sA = air ? "<color=#fff>💨 Aire: PURIFICADO</color>" : "<color=#888>💨 Aire: Corrompido</color>";
            string sE = earth ? "<color=#fa4>🏜️ Tierra: PURIFICADA</color>" : "<color=#888>🏜️ Tierra: Corrompida</color>";

            GUI.Label(new Rect(rect.x, rect.y + 36, rect.width, 60), $"{sW}   |   {sF}\n{sA}   |   {sE}", styleLines);

            if (count == 4)
            {
                GUIStyle alert = new GUIStyle(GUI.skin.label);
                alert.alignment = TextAnchor.MiddleCenter;
                alert.fontStyle = FontStyle.Bold;
                alert.fontSize = 12;
                alert.normal.textColor = Color.yellow;
                GUI.Label(new Rect(rect.x, rect.y + 104, rect.width, 26), "¡Toca el Altar para despertar el Árbol!", alert);
            }
            else
            {
                GUI.Label(new Rect(rect.x, rect.y + 104, rect.width, 26), "Derrota a los 4 jefes para restaurar el mundo.", styleLines);
            }
        }
    }
}
