using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona upgrades comprados y aplica efectos al PlayerData.
/// Los niveles se persisten con PlayerPrefs.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private List<UpgradeData> allUpgrades;
    [SerializeField] private PlayerData playerData;

    private DriftEssenceManager essenceManager;
    private GameConfig gameConfig;

    private void Start()
    {
        essenceManager = GetComponent<DriftEssenceManager>();
        gameConfig = GameManager.Instance.Config;
        ApplyAllUpgrades();
    }

    // ─── Consultas ────────────────────────────────────────────────────

    public int GetLevel(UpgradeData upgrade)
    {
        return PlayerPrefs.GetInt($"Upgrade_{upgrade.upgradeType}", 0);
    }

    public bool IsMaxed(UpgradeData upgrade)
    {
        return GetLevel(upgrade) >= upgrade.maxLevel;
    }

    public float GetNextCost(UpgradeData upgrade)
    {
        return upgrade.GetCostForLevel(GetLevel(upgrade));
    }

    public List<UpgradeData> GetAllUpgrades() => allUpgrades;

    // ─── Compra ───────────────────────────────────────────────────────

    public bool TryPurchase(UpgradeData upgrade)
    {
        if (IsMaxed(upgrade)) return false;

        float cost = GetNextCost(upgrade);
        if (!essenceManager.TrySpend(cost)) return false;

        int newLevel = GetLevel(upgrade) + 1;
        PlayerPrefs.SetInt($"Upgrade_{upgrade.upgradeType}", newLevel);
        PlayerPrefs.Save();

        ApplyAllUpgrades();

        EventBus.Publish(new UpgradePurchasedEvent
        {
            upgradeId = upgrade.upgradeType.ToString(),
            newLevel = newLevel
        });

        Debug.Log($"[Upgrades] {upgrade.upgradeName} → Nivel {newLevel}");
        return true;
    }

    // ─── Aplicar efectos ──────────────────────────────────────────────

    /// <summary>
    /// Recalcula todos los stats del PlayerData aplicando los upgrades comprados.
    /// Siempre parte desde los valores base de GameConfig.
    /// </summary>
    private void ApplyAllUpgrades()
    {
        // Resetear a valores base
        playerData.ResetToBase(gameConfig);

        // Aplicar cada upgrade según su nivel actual
        foreach (UpgradeData upgrade in allUpgrades)
        {
            int level = GetLevel(upgrade);
            if (level == 0) continue;

            float totalEffect = upgrade.effectPerLevel * level;
            ApplyEffect(upgrade.upgradeType, level, totalEffect);
        }
    }

    private void ApplyEffect(UpgradeType type, int level, float totalEffect)
    {
        switch (type)
        {
            case UpgradeType.MaxSpeed:
                playerData.maxSpeed *= (1f + totalEffect);
                break;

            case UpgradeType.Acceleration:
                playerData.thrustForce *= (1f + totalEffect);
                break;

            case UpgradeType.HitboxReduction:
                playerData.hitboxScale = Mathf.Max(0.5f, 1f - totalEffect);
                break;

            case UpgradeType.StartingShield:
                playerData.hasStartingShield = level > 0;
                break;

            case UpgradeType.ExtraLife:
                playerData.extraLives = level;
                break;

                // GhostDash, ScoreMultiplier y NearMissBonus
                // se aplican en sus sistemas correspondientes
                // leyendo GetLevel() directamente desde UpgradeManager
        }
    }
}
