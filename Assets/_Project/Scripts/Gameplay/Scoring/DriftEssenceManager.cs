using UnityEngine;

/// <summary>
/// Gestiona la Drift Essence acumulada entre runs.
/// Persiste con PlayerPrefs — sobrevive al cierre del juego.
/// </summary>
public class DriftEssenceManager : MonoBehaviour
{
    private const string EssenceKey = "DriftEssence";

    // Acceso de solo lectura
    public float TotalEssence => PlayerPrefs.GetFloat(EssenceKey, 0f);

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnPlayerDied(PlayerDiedEvent e)
    {
        if (e.driftEssenceEarned <= 0f) return;

        float current = TotalEssence;
        float updated = current + e.driftEssenceEarned;

        PlayerPrefs.SetFloat(EssenceKey, updated);
        PlayerPrefs.Save();

        Debug.Log($"[DriftEssence] Ganada:{e.driftEssenceEarned:F1} " +
                  $"Total acumulado:{updated:F1}");
    }

    /// <summary>
    /// Gasta Drift Essence. Retorna false si no hay suficiente.
    /// Usado por el sistema de upgrades en el Step 8.
    /// </summary>
    public bool TrySpend(float amount)
    {
        if (TotalEssence < amount) return false;

        PlayerPrefs.SetFloat(EssenceKey, TotalEssence - amount);
        PlayerPrefs.Save();
        return true;
    }

    /// <summary>
    /// Resetea toda la Drift Essence — usar solo para debug o reset completo.
    /// </summary>
    public void ResetEssence()
    {
        PlayerPrefs.SetFloat(EssenceKey, 0f);
        PlayerPrefs.Save();
        Debug.Log("[DriftEssence] Reseteada a 0");
    }
}
