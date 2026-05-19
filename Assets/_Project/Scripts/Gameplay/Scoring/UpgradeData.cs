using UnityEngine;

public enum UpgradeType
{
    MaxSpeed,
    Acceleration,
    HitboxReduction,
    ScoreMultiplier,
    GhostDash,
    StartingShield,
    NearMissBonus,
    ExtraLife
}

/// <summary>
/// Define un upgrade permanente. Crear un asset por upgrade.
/// Los valores se basan en Upgrades.md de la bóveda de Obsidian.
/// </summary>
[CreateAssetMenu(fileName = "UpgradeData", menuName = "VoidDrift/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Identificación")]
    public UpgradeType upgradeType;
    public string upgradeName;
    [TextArea] public string effectDescription;

    [Header("Economía")]
    public float baseCost = 80f;
    public int maxLevel = 5;

    [Header("Efecto por nivel")]
    [Tooltip("Valor que se aplica por nivel (velocidad, fuerza, etc.)")]
    public float effectPerLevel = 0.15f;

    /// <summary>
    /// Costo del próximo nivel — aumenta con cada nivel comprado.
    /// </summary>
    public float GetCostForLevel(int currentLevel)
    {
        return baseCost * (currentLevel + 1);
    }
}
