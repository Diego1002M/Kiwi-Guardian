using UnityEngine;

/// <summary>
/// Interfaz universal para cualquier entidad que pueda recibir daño (enemigos, jefes, cajas de chatarra).
/// </summary>
public interface IDamageable
{
    void TakeDamage(int damageAmount, Vector2 hitDirection);
}
