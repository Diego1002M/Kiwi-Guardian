using System.Collections;
using UnityEngine;

/// <summary>
/// Muro de raíces y ramas muertas que bloquea caminos en el Bosque Marchito.
/// SE QUEMA Y DESTRUYE al ser golpeado por dagas de FUEGO [2 / Q].
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WitheredRootBarrier : MonoBehaviour, IElementalTarget
{
    [SerializeField] private Color normalColor = new Color(0.38f, 0.28f, 0.2f);
    [SerializeField] private Color burningColor = new Color(1f, 0.4f, 0.1f);

    private bool isBurning = false;
    private SpriteRenderer sr;
    private Collider2D col;

    public ElementType TargetElement => ElementType.Tierra;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (sr != null) sr.color = normalColor;
    }

    public void ApplyElementalHit(ElementType element, int damage, Vector2 hitDirection)
    {
        if (isBurning) return;

        // 🔥 El fuego quema las raíces secas
        if (element == ElementType.Fuego)
        {
            BurnDown();
        }
        else
        {
            StartCoroutine(ResistShake());
        }
    }

    private IEnumerator ResistShake()
    {
        if (sr != null) sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        if (sr != null && !isBurning) sr.color = normalColor;
    }

    private void BurnDown()
    {
        isBurning = true;
        KiwiAudio.PlayEnemyHit();
        Debug.Log("🔥 ¡Las raíces secas se han quemado con la Llama Solar!");

        if (col != null) col.enabled = false;
        StartCoroutine(BurnAnim());
    }

    private IEnumerator BurnAnim()
    {
        float timer = 0f;
        float duration = 0.5f;

        if (sr != null) sr.color = burningColor;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (sr != null)
            {
                Color c = Color.Lerp(burningColor, Color.black, t);
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
