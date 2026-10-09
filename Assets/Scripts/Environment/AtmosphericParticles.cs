using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Generador procedural de partículas atmosféricas vivas para cada bioma de Kiwi:
/// - Mar de Plástico: Burbujas submarinas y plancton bioluminiscente.
/// - Ciudad de Humo: Motas de smog, cenizas y vapor.
/// - Desierto de Residuos: Brasas incandescentes y chispas de calor.
/// - Bosque Marchito: Esporas mágicas flotantes y hojas de otoño.
/// - Páramo Tóxico: Niebla química violeta.
/// </summary>
public class AtmosphericParticles : MonoBehaviour
    {
        [SerializeField] private int particleCount = 45;

        private struct ParticleMote
        {
            public Vector3 position;
            public Vector3 velocity;
            public float size;
            public float alpha;
            public float lifetime;
            public float maxLifetime;
            public Color color;
        }

        private ParticleMote[] motes;
        private Camera targetCamera;
        private Texture2D particleTex;
        private GUIStyle moteStyle;

        private Color biomeColorPrimary = new Color(0.2f, 0.85f, 1f, 0.6f);
        private Color biomeColorSecondary = new Color(0.1f, 0.95f, 0.65f, 0.5f);
        private float riseSpeed = 0.8f;
        private float driftSpeed = 0.4f;

        private void Start()
        {
            targetCamera = Camera.main;
            ConfigureBiomeStyle();
            InitializeMotes();
        }

        private void ConfigureBiomeStyle()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName.Contains("MarDePlastico") || sceneName.Contains("NivelPruebas"))
            {
                // Burbujas y plancton marino
                biomeColorPrimary = new Color(0.2f, 0.85f, 1f, 0.65f); // Cian agua
                biomeColorSecondary = new Color(0.6f, 0.95f, 1f, 0.5f); // Blanco burbuja
                riseSpeed = 0.7f;
                driftSpeed = 0.35f;
            }
            else if (sceneName.Contains("CiudadDeHumo"))
            {
                // Cenizas y vapor industrial
                biomeColorPrimary = new Color(0.7f, 0.72f, 0.8f, 0.55f);
                biomeColorSecondary = new Color(1f, 0.6f, 0.2f, 0.4f);
                riseSpeed = 0.5f;
                driftSpeed = 0.7f;
            }
            else if (sceneName.Contains("DesiertoDeResiduos"))
            {
                // Brasas incandescentes y chispas
                biomeColorPrimary = new Color(1f, 0.45f, 0.1f, 0.75f); // Naranja fuego
                biomeColorSecondary = new Color(1f, 0.85f, 0.2f, 0.65f); // Amarillo brasa
                riseSpeed = 0.9f;
                driftSpeed = 0.5f;
            }
            else if (sceneName.Contains("BosqueMarchito"))
            {
                // Esporas de savia y polen místico
                biomeColorPrimary = new Color(0.3f, 0.95f, 0.45f, 0.7f); // Verde savia
                biomeColorSecondary = new Color(0.9f, 0.75f, 0.25f, 0.5f); // Hoja seca
                riseSpeed = 0.4f;
                driftSpeed = 0.25f;
            }
            else // ParamoToxico
            {
                // Niebla y efluvio tóxico
                biomeColorPrimary = new Color(0.65f, 0.35f, 0.85f, 0.6f);
                biomeColorSecondary = new Color(0.3f, 0.85f, 0.65f, 0.45f);
                riseSpeed = 0.5f;
                driftSpeed = 0.4f;
            }

            // Generar textura circular suave para las motas
            particleTex = new Texture2D(12, 12, TextureFormat.RGBA32, false);
            for (int x = 0; x < 12; x++)
            {
                for (int y = 0; y < 12; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(5.5f, 5.5f));
                    if (dist <= 5.5f)
                    {
                        float a = Mathf.Clamp01(1f - (dist / 5.5f));
                        particleTex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
                    }
                    else
                    {
                        particleTex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            particleTex.Apply();
        }

        private void InitializeMotes()
        {
            motes = new ParticleMote[particleCount];
            for (int i = 0; i < particleCount; i++)
            {
                ResetMote(ref motes[i], true);
            }
        }

        private void ResetMote(ref ParticleMote mote, bool randomY)
        {
            Vector3 camPos = (targetCamera != null) ? targetCamera.transform.position : Vector3.zero;
            float camW = (targetCamera != null) ? targetCamera.orthographicSize * 2f * targetCamera.aspect + 6f : 24f;
            float camH = (targetCamera != null) ? targetCamera.orthographicSize * 2f + 4f : 16f;

            float x = camPos.x + Random.Range(-camW / 2f, camW / 2f);
            float y = randomY ? (camPos.y + Random.Range(-camH / 2f, camH / 2f)) : (camPos.y - camH / 2f - Random.Range(0f, 1.5f));

            mote.position = new Vector3(x, y, 0f);
            mote.velocity = new Vector3(Random.Range(-driftSpeed, driftSpeed), Random.Range(riseSpeed * 0.6f, riseSpeed * 1.4f), 0f);
            mote.size = Random.Range(6f, 14f);
            mote.maxLifetime = Random.Range(4f, 8f);
            mote.lifetime = randomY ? Random.Range(0f, mote.maxLifetime) : 0f;
            mote.color = (Random.value > 0.5f) ? biomeColorPrimary : biomeColorSecondary;
        }

        private void Update()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (motes == null) return;

            Vector3 camPos = (targetCamera != null) ? targetCamera.transform.position : Vector3.zero;
            float camH = (targetCamera != null) ? targetCamera.orthographicSize * 2f + 4f : 16f;

            for (int i = 0; i < motes.Length; i++)
            {
                motes[i].lifetime += Time.deltaTime;
                motes[i].position += (motes[i].velocity + new Vector3(Mathf.Sin(Time.time * 2f + i) * 0.3f, 0, 0)) * Time.deltaTime;

                // Reaparecer si sale de pantalla o expira
                if (motes[i].lifetime >= motes[i].maxLifetime || motes[i].position.y > camPos.y + camH / 2f + 1f)
                {
                    ResetMote(ref motes[i], false);
                }
            }
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;
            if (targetCamera == null || motes == null || particleTex == null) return;

            for (int i = 0; i < motes.Length; i++)
            {
                Vector3 screenPos = targetCamera.WorldToScreenPoint(motes[i].position);
                if (screenPos.z > 0 && screenPos.x >= -30 && screenPos.x <= Screen.width + 30 && screenPos.y >= -30 && screenPos.y <= Screen.height + 30)
                {
                    float lifeRatio = motes[i].lifetime / motes[i].maxLifetime;
                    float fade = Mathf.Sin(lifeRatio * Mathf.PI); // Aparece y desaparece suavemente
                    Color c = motes[i].color;
                    c.a *= fade;

                    GUI.color = c;
                    float s = motes[i].size;
                    GUI.DrawTexture(new Rect(screenPos.x - s / 2f, Screen.height - screenPos.y - s / 2f, s, s), particleTex);
                }
            }
            GUI.color = Color.white;
        }
    }
