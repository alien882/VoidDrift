using UnityEngine;

/// <summary>
/// Configuración central del juego. Editable desde el Inspector sin tocar código.
/// Valores basados en Balance.md de la bóveda de Obsidian.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "VoidDrift/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Jugador")]
    public float playerMaxSpeed = 11f;
    public float playerThrustForce = 32f;
    public float playerLinearDrag = 1.8f;
    public float playerAngularDrag = 2.5f;

    [Header("Near Miss")]
    public float nearMissDistance = 1.5f;
    public float nearMissCooldown = 0.5f;
    public int nearMissBasePoints = 15;

    [Header("Scoring")]
    public float multiplierIncreaseInterval = 45f;  // Segundos entre cada aumento
    public float multiplierIncreaseAmount = 0.5f;   // Cuánto sube cada vez

    [Header("Drift Essence")]
    public float essenceTimeMultiplier = 0.85f;
    public float essenceMultiplierBonus = 25f;
    public float essenceNearMissBonus = 8f;

    [Header("Dificultad — Spawn Rate (segundos entre spawns)")]
    public float spawnRatePhase1 = 1.8f;    // 0–30s
    public float spawnRatePhase2 = 1.4f;    // 30–60s
    public float spawnRatePhase3 = 1.1f;    // 60–90s
    public float spawnRatePhase4 = 0.9f;    // 90–120s
    public float spawnRatePhase5 = 0.7f;    // 120s+

    [Header("Dificultad — Velocidad de Asteroides (multiplicador)")]
    public float asteroidSpeedPhase1 = 1.0f;
    public float asteroidSpeedPhase2 = 1.15f;
    public float asteroidSpeedPhase3 = 1.30f;
    public float asteroidSpeedPhase4 = 1.45f;
    public float asteroidSpeedPhase5 = 1.60f;

    /// <summary>
    /// Calcula la Drift Essence ganada al final de una run.
    /// Fórmula de Balance.md: (Tiempo × 0.85) + (Multiplicador × 25) + (NearMiss × 8)
    /// </summary>
    public float CalculateDriftEssence(float survivalTime, float maxMultiplier, int nearMissCount)
    {
        return (survivalTime * essenceTimeMultiplier)
             + (maxMultiplier * essenceMultiplierBonus)
             + (nearMissCount * essenceNearMissBonus);
    }
}
