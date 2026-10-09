using UnityEngine;

/// <summary>
/// Animador por sprites de Kiwi basado en el SpriteSheet HD del usuario:
/// - Reposo (Idle): Gato naranja con chaqueta teal y mochila con gema.
/// - Carrera (Run): Postura dinámica de carrera inclinada hacia adelante.
/// - Salto / En el aire (Jump): Postura atlética con patas recogidas.
/// - Ataque (Attack): Extiende el brazo lanzando las dagas elementales.
/// - Caminata (Walk): Ciclo de 4 cuadros de pasos suaves.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class KiwiSpriteAnimator : MonoBehaviour
{
    [Header("Sprites de Animación HD")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite runSprite;
    [SerializeField] private Sprite jumpSprite;
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite[] walkSprites;

    [Header("Parámetros")]
    [SerializeField] private float walkFps = 7.5f;

    private SpriteRenderer sr;
    private KiwiController controller;
    private KiwiCombat combat;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        controller = GetComponent<KiwiController>();
        combat = GetComponent<KiwiCombat>();

        LoadDefaultSpritesIfNull();
    }

    private void Start()
    {
        LoadDefaultSpritesIfNull();
    }

    public void SetupSprites(Sprite idle, Sprite run, Sprite jump, Sprite attack, Sprite[] walk)
    {
        idleSprite = idle;
        runSprite = run;
        jumpSprite = jump;
        attackSprite = attack;
        walkSprites = walk;
    }

    private void LoadDefaultSpritesIfNull()
    {
        #if UNITY_EDITOR
        if (idleSprite == null)
            idleSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/KiwiCat.png");
        if (runSprite == null)
            runSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/KiwiCat_Run.png");
        if (jumpSprite == null)
            jumpSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/KiwiCat_Jump.png");
        if (attackSprite == null)
            attackSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/KiwiCat_Attack.png");

        if (walkSprites == null || walkSprites.Length == 0)
        {
            walkSprites = new Sprite[4];
            walkSprites[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/KiwiCat_Walk1.png");
            walkSprites[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/KiwiCat_Walk2.png");
            walkSprites[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/KiwiCat_Walk3.png");
            walkSprites[3] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/KiwiCat_Walk4.png");
        }
        #endif

        if (idleSprite == null && sr != null)
        {
            idleSprite = sr.sprite;
        }
    }

    private void LateUpdate()
    {
        if (sr == null) return;

        // 1. Estado de Ataque
        if (combat != null && combat.IsAttacking)
        {
            sr.sprite = attackSprite != null ? attackSprite : idleSprite;
            return;
        }

        // 2. Estado de Salto / En el aire
        if (controller != null && !controller.IsGrounded)
        {
            sr.sprite = jumpSprite != null ? jumpSprite : idleSprite;
            return;
        }

        // 3. Estado de Movimiento Horizontal (Caminar / Correr)
        float moveInput = Mathf.Abs(Input.GetAxisRaw("Horizontal"));
        if (moveInput > 0.08f)
        {
            if (walkSprites != null && walkSprites.Length > 0 && walkSprites[0] != null)
            {
                int frame = (int)(Time.time * walkFps) % walkSprites.Length;
                sr.sprite = walkSprites[frame];
            }
            else if (runSprite != null)
            {
                sr.sprite = runSprite;
            }
            else
            {
                sr.sprite = idleSprite;
            }
            return;
        }

        // 4. Estado de Reposo (Idle)
        sr.sprite = idleSprite;
    }
}
