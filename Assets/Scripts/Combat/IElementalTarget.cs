using UnityEngine;

/// <summary>
/// Interfaz para entidades con afinidad elemental (enemigos, muros de fuego, raíces).
/// </summary>
public interface IElementalTarget
{
    ElementType TargetElement { get; }
    void ApplyElementalHit(ElementType element, int damage, Vector2 hitDirection);
}
