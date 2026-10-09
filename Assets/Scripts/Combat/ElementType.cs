using UnityEngine;

/// <summary>
/// Tipos de elementos sagrados de la Última Semilla.
/// </summary>
public enum ElementType
{
    Agua,   // 🌊 Purifica y apaga fuego, enfría lava
    Aire,   // 💨 Dispersa smog y corta viento
    Fuego,  // 🔥 Quema raíces y derrite obstáculos
    Tierra  // 🌱 Rompe blindajes y fertiliza brotes
}

public static class ElementalHelpers
{
    public static Color GetColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Agua: return new Color(0.2f, 0.85f, 1f);
            case ElementType.Fuego: return new Color(1f, 0.4f, 0.1f);
            case ElementType.Aire: return new Color(1f, 0.95f, 0.5f);
            case ElementType.Tierra: return new Color(0.25f, 0.9f, 0.4f);
            default: return Color.white;
        }
    }

    public static string GetIcon(ElementType element)
    {
        switch (element)
        {
            case ElementType.Agua: return "🌊 Agua";
            case ElementType.Fuego: return "🔥 Fuego";
            case ElementType.Aire: return "💨 Aire";
            case ElementType.Tierra: return "🌱 Tierra";
            default: return "Normal";
        }
    }

    public static bool IsStrongAgainst(ElementType attacker, ElementType defender)
    {
        // 🌊 Agua apaga 🔥 Fuego
        if (attacker == ElementType.Agua && defender == ElementType.Fuego) return true;
        // 🔥 Fuego quema 🌱 Tierra
        if (attacker == ElementType.Fuego && defender == ElementType.Tierra) return true;
        // 🌱 Tierra absorbe 🌊 Agua
        if (attacker == ElementType.Tierra && defender == ElementType.Agua) return true;
        // 💨 Aire aviva o dispersa
        if (attacker == ElementType.Aire && defender == ElementType.Tierra) return true;

        return false;
    }
}
