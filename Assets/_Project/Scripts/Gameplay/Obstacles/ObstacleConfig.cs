using UnityEngine;

/// <summary>
/// Configuración de un tipo de asteroide.
/// Crear un asset por tipo: Big, Medium, Small.
/// </summary>
[CreateAssetMenu(fileName = "ObstacleConfig", menuName = "VoidDrift/Obstacle Config")]
public class ObstacleConfig : ScriptableObject
{
    [Header("Visual")]
    public Sprite sprite;

    [Header("Tamaño")]
    public float minScale = 0.6f;
    public float maxScale = 0.9f;

    [Header("Velocidad (units/s)")]
    public float minSpeed = 2f;
    public float maxSpeed = 4.5f;

    [Header("Pool")]
    public int poolDefaultCapacity = 10;
    public int poolMaxSize = 30;
}
