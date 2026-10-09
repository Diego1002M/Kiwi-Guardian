using UnityEngine;

/// <summary>
/// Sistema de combate elemental de Kiwi:
/// Permite alternar entre elementos (Agua, Fuego, Aire, Tierra) con [Q] o las teclas [1, 2, 3, 4],
/// disparando dagas cargadas con el elemento activo.
/// </summary>
public class KiwiCombat : MonoBehaviour
{
    [Header("Ataque Básico (Dagas)")]
    [SerializeField] private GameObject daggerPrefab;
    [SerializeField] private Transform attackSpawnPoint;
    [SerializeField] private float attackCooldown = 0.28f;

    [Header("Poder Elemental")]
    public ElementType currentElement = ElementType.Agua;
    public bool hasTripleDagger = false; // Desbloqueable en la tienda del Mapache

    private float nextAttackTime = 0f;
    private float lastAttackTime = -10f;
    private KiwiController controller;

    public ElementType CurrentElement => currentElement;
    public bool IsAttacking => Time.time < lastAttackTime + 0.22f;

    private void Awake()
    {
        controller = GetComponent<KiwiController>();
    }

    public void SetDaggerPrefab(GameObject prefab)
    {
        daggerPrefab = prefab;
    }

    private void Update()
    {
        // 1. Selector de Elementos con tecla [Q] o teclas [1, 2, 3, 4]
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleElement();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetElement(ElementType.Agua);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetElement(ElementType.Fuego);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetElement(ElementType.Aire);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetElement(ElementType.Tierra);
        }

        // 2. Atacar con clic izquierdo, teclas J, K, X o F
        bool attackPressed = Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.F);
        if (attackPressed && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void CycleElement()
    {
        int next = ((int)currentElement + 1) % 4;
        SetElement((ElementType)next);
    }

    public void SetElement(ElementType newElement)
    {
        currentElement = newElement;
        KiwiAudio.PlayDoubleJump();
        Debug.Log($"🌀 Elemento de Kiwi cambiado a: {ElementalHelpers.GetIcon(currentElement)}");
    }

    private void Attack()
    {
        lastAttackTime = Time.time;
        if (daggerPrefab == null) return;

        Vector3 spawnPos = attackSpawnPoint != null ? attackSpawnPoint.position : transform.position + (controller.IsFacingRight ? Vector3.right : Vector3.left) * 0.5f;

        SpawnDagger(spawnPos, 0f);

        // Si compró la mejora de dagas triples en la tienda del Mapache
        if (hasTripleDagger)
        {
            SpawnDagger(spawnPos, 12f);
            SpawnDagger(spawnPos, -12f);
        }

        KiwiAudio.PlayDagger();
    }

    private void SpawnDagger(Vector3 pos, float angleOffset)
    {
        GameObject daggerObj = Instantiate(daggerPrefab, pos, Quaternion.Euler(0, 0, angleOffset));
        DaggerProjectile dagger = daggerObj.GetComponent<DaggerProjectile>();
        if (dagger != null)
        {
            dagger.Setup(controller.IsFacingRight, currentElement);
        }
    }
}
