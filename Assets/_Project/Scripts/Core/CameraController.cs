using UnityEngine;

/// <summary>
/// Controla la cámara ortográfica para que el área de juego
/// sea siempre visible en cualquier resolución y aspecto.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Área de juego base")]
    [Tooltip("Mitad del ancho del área de juego en unidades. " +
                 "El juego siempre mostrará al menos este ancho.")]
    [SerializeField] private float baseHalfWidth = 9f;

    [Tooltip("Mitad de la altura del área de juego en unidades. " +
             "El juego siempre mostrará al menos esta altura.")]
    [SerializeField] private float baseHalfHeight = 5f;

    private Camera cam;

    // Límites del área visible — otros sistemas los usan para spawn y bordes
    public float WorldTop { get; private set; }
    public float WorldBottom { get; private set; }
    public float WorldLeft { get; private set; }
    public float WorldRight { get; private set; }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        AdjustCamera();
    }

    private void AdjustCamera()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float targetAspect = baseHalfWidth / baseHalfHeight;

        if (screenAspect >= targetAspect)
        {
            // Pantalla más ancha que el área base (landscape, WebGL)
            // Ajustamos por altura — el ancho sobrante queda como margen
            cam.orthographicSize = baseHalfHeight;
        }
        else
        {
            // Pantalla más alta que el área base (portrait, móvil)
            // Ajustamos por ancho — aseguramos que todo el ancho sea visible
            cam.orthographicSize = baseHalfWidth / screenAspect;
        }

        UpdateWorldBounds();
    }

    private void UpdateWorldBounds()
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        WorldTop = halfHeight;
        WorldBottom = -halfHeight;
        WorldLeft = -halfWidth;
        WorldRight = halfWidth;
    }

    /// <summary>
    /// Recalcula la cámara cuando cambia el tamaño de la pantalla.
    /// Útil en WebGL cuando el usuario redimensiona el navegador.
    /// </summary>
    private void Update()
    {
        // Solo recalcula si el aspecto cambió (evita cálculo cada frame)
        float currentAspect = (float)Screen.width / Screen.height;
        if (!Mathf.Approximately(currentAspect, cam.aspect))
        {
            AdjustCamera();
        }
    }

    /// <summary>
    /// Verifica si una posición en el mundo está fuera del área visible.
    /// Útil para el spawner de asteroides y para detectar muerte por borde.
    /// </summary>
    public bool IsOutOfBounds(Vector2 worldPosition, float margin = 0f)
    {
        return worldPosition.x < WorldLeft - margin ||
               worldPosition.x > WorldRight + margin ||
               worldPosition.y < WorldBottom - margin ||
               worldPosition.y > WorldTop + margin;
    }

    /// <summary>
    /// Devuelve una posición aleatoria fuera del área visible.
    /// Usada por el ObstacleSpawner en el Step 5.
    /// </summary>
    public Vector2 GetRandomSpawnPosition(float spawnMargin = 1f)
    {
        int side = Random.Range(0, 4); // 0=top, 1=bottom, 2=left, 3=right

        return side switch
        {
            0 => new Vector2(Random.Range(WorldLeft, WorldRight), WorldTop + spawnMargin),
            1 => new Vector2(Random.Range(WorldLeft, WorldRight), WorldBottom - spawnMargin),
            2 => new Vector2(WorldLeft - spawnMargin, Random.Range(WorldBottom, WorldTop)),
            _ => new Vector2(WorldRight + spawnMargin, Random.Range(WorldBottom, WorldTop)),
        };
    }

#if UNITY_EDITOR
    // Dibuja el área de juego en el Editor para visualizar los límites
    private void OnDrawGizmos()
    {
        if (cam == null) cam = GetComponent<Camera>();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(baseHalfWidth * 2, baseHalfHeight * 2, 0)
        );
    }
#endif
}
