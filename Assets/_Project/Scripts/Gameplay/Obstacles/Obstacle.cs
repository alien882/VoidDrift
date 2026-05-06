using UnityEngine;

/// <summary>
/// Comportamiento de un asteroide individual.
/// Se mueve en línea recta y se devuelve al pool al salir de pantalla.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Obstacle : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private CameraController cameraController;

    // Referencia al pool para auto-devolverse
    private System.Action<Obstacle> returnToPool;

    // Margen fuera de pantalla antes de devolverse al pool
    private const float OutOfBoundsMargin = 2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    /// <summary>
    /// Inicializa el asteroide con su configuración y dirección.
    /// Llamado por el ObstacleSpawner al sacarlo del pool.
    /// </summary>
    public void Initialize(ObstacleConfig config, Vector2 spawnPosition,
                           Vector2 direction, float speedMultiplier,
                           System.Action<Obstacle> onReturnToPool)
    {
        returnToPool = onReturnToPool;

        // Configurar sprite
        spriteRenderer.sprite = config.sprite;

        // Tamaño aleatorio dentro del rango del config
        float scale = Random.Range(config.minScale, config.maxScale);
        transform.localScale = Vector3.one * scale;

        // Posición
        transform.position = spawnPosition;

        // Rotación inicial aleatoria
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        // Velocidad aplicada como velocidad directa (no AddForce)
        // Así los valores de Balance.md son exactos sin depender de la masa
        float speed = Random.Range(config.minSpeed, config.maxSpeed) * speedMultiplier;
        rb.linearVelocity = direction * speed;

        // Rotación visual continua
        rb.angularVelocity = Random.Range(-90f, 90f);
    }

    private void Update()
    {
        // Auto-devolver al pool cuando sale de pantalla
        if (cameraController != null &&
            cameraController.IsOutOfBounds(transform.position, OutOfBoundsMargin))
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        returnToPool?.Invoke(this);
    }
}
