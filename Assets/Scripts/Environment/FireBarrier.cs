using System.Collections;
using UnityEngine;
using Platformer2D;

/// <summary>
/// Muro o cortina de fuego contaminado que bloquea caminos en el Desierto de Residuos.
/// SE APAGA al ser golpeado por agua (dagas de agua o atravesado con el Torbellino Acuático).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FireBarrier : MonoBehaviour, IElementalTarget
{
    [Header("Configuración")]
    [SerializeField] private int hitsToExtinguish = 1;
    [SerializeField] private Color fireColor = new Color(1f, 0.4f, 0.1f, 0.95f);
    [SerializeField] private Color steamColor = new Color(0.8f, 0.9f, 1f, 0.4f);

    private int currentHits = 0;
    private bool isExtinguished = false;
    private SpriteRenderer sr;
    private Collider2D col;

    public ElementType TargetElement => ElementType.Fuego;
    public bool IsExtinguished => isExtinguished;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (sr != null) sr.color = fireColor;
    }

    private void Update()
    {
        if (isExtinguished) return;

        // Efecto parpadeante de fuego
        if (sr != null)
        {
            float flicker = 0.8f + Mathf.PingPong(Time.time * 6f, 0.35f);
            Color c = fireColor;
            c.a = flicker;
            sr.color = c;
        }
    }

    public void ApplyElementalHit(ElementType element, int damage, Vector2 hitDirection)
    {
        if (isExtinguished) return;

        // 🌊 El agua apaga el fuego
        if (element == ElementType.Agua)
        {
            currentHits += damage;
            if (currentHits >= hitsToExtinguish)
            {
                Extinguish();
            }
        }
        else
        {
            // Otros elementos no apagan el fuego, chisporrotean
            StartCoroutine(ResistFlicker());
        }
    }

    private IEnumerator ResistFlicker()
    {
        if (sr != null) sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        if (sr != null && !isExtinguished) sr.color = fireColor;
    }

    private void Extinguish()
    {
        isExtinguished = true;
        KiwiAudio.PlayFlyPop(); // Sonido de vapor sssss
        Debug.Log("🌊 ¡El muro de fuego ha sido apagado con agua purificada!");

        if (col != null) col.enabled = false;

        StartCoroutine(SteamDissolve());
    }

    private IEnumerator SteamDissolve()
    {
        float timer = 0f;
        float duration = 0.45f;
        Vector3 origScale = transform.localScale;

        if (sr != null) sr.color = steamColor;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // El vapor se eleva y se desvanece
            transform.position += Vector3.up * 1.2f * Time.deltaTime;
            transform.localScale = Vector3.Lerp(origScale, origScale * 1.4f, t);

            if (sr != null)
            {
                Color c = steamColor;
                c.a = Mathf.Lerp(0.6f, 0f, t);
                sr.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isExtinguished) return;

        // Si Kiwi lo toca
        if (collision.CompareTag("Player"))
        {
            // Daño por quemadura
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(1, transform.position);
            }
        }
    }
}
