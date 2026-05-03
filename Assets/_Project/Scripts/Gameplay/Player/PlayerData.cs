using UnityEngine;

/// <summary>
/// Estado del jugador entre runs. Modificado por el sistema de upgrades.
/// Los valores base vienen de GameConfig — los upgrades los incrementan.
/// </summary>
[CreateAssetMenu(fileName = "PlayerData", menuName = "VoidDrift/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Stats actuales (base + upgrades)")]
    public float maxSpeed = 11f;
    public float thrustForce = 32f;
    public float linearDrag = 1.8f;
    public float angularDrag = 2.5f;
    public float hitboxScale = 1f;    // 1 = tamaño original, 0.9 = 10% más pequeño
    public bool hasStartingShield = false;
    public int extraLives = 0;

    /// <summary>
    /// Resetea los stats al valor base de GameConfig.
    /// Llamar cuando el jugador reinicia desde cero (no entre runs).
    /// </summary>
    public void ResetToBase(GameConfig config)
    {
        maxSpeed = config.playerMaxSpeed;
        thrustForce = config.playerThrustForce;
        linearDrag = config.playerLinearDrag;
        angularDrag = config.playerAngularDrag;
        hitboxScale = 1f;
        hasStartingShield = false;
        extraLives = 0;
    }
}
