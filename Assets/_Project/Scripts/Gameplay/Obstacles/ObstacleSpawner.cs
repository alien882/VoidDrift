using UnityEngine;

/// <summary>
/// Spawner de asteroides con Object Pooling y curva de dificultad progresiva.
/// Lee los valores de dificultad directamente desde GameConfig.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Configuraciones de asteroides")]
    [SerializeField] private ObstacleConfig configSmall;
    [SerializeField] private ObstacleConfig configMedium;
    [SerializeField] private ObstacleConfig configBig;

    [Header("Prefab base (sin configuración)")]
    [SerializeField] private Obstacle obstaclePrefab;

    // Pools
    private GenericPool<Obstacle> poolSmall;
    private GenericPool<Obstacle> poolMedium;
    private GenericPool<Obstacle> poolBig;

    // Referencias
    private CameraController cameraController;
    private GameConfig gameConfig;

    // Estado
    private float spawnTimer;
    private float currentSpawnRate;
    private float currentSpeedMultiplier;
    private float elapsedTime;
    private bool isSpawning;

    // Probabilidades de spawn por tipo (deben sumar 1.0)
    private const float SpawnChanceSmall = 0.45f;
    private const float SpawnChanceMedium = 0.40f;
    // Big = 1 - Small - Medium = 0.15f

    // Estado del Asteroid Storm
    private bool stormActive;
    private float stormTimer;
    private int stormAsteroidsRemaining;
    private const float StormSpawnInterval = 0.15f;

    private void OnEnable()
    {
        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Unsubscribe<GameStateChangedEvent>(OnStateChanged);
    }

    private void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        gameConfig = GameManager.Instance.Config;
        InitializePools();
    }

    private void InitializePools()
    {
        // Crear un Transform contenedor para mantener la jerarquía limpia
        Transform poolContainer = new GameObject("ObstaclePool").transform;
        poolContainer.SetParent(transform);

        poolSmall = new GenericPool<Obstacle>(obstaclePrefab, poolContainer,
                         configSmall.poolDefaultCapacity, configSmall.poolMaxSize);
        poolMedium = new GenericPool<Obstacle>(obstaclePrefab, poolContainer,
                         configMedium.poolDefaultCapacity, configMedium.poolMaxSize);
        poolBig = new GenericPool<Obstacle>(obstaclePrefab, poolContainer,
                         configBig.poolDefaultCapacity, configBig.poolMaxSize);
    }

    private void Update()
    {
        if (!isSpawning) return;

        elapsedTime += Time.deltaTime;
        spawnTimer += Time.deltaTime;

        UpdateDifficulty();

        // Spawn normal
        if (spawnTimer >= currentSpawnRate)
        {
            spawnTimer = 0f;
            SpawnObstacle();
        }

        // Spawn del storm
        if (stormActive)
        {
            stormTimer += Time.deltaTime;
            if (stormTimer >= StormSpawnInterval)
            {
                stormTimer = 0f;
                SpawnStormObstacle();
                stormAsteroidsRemaining--;
                if (stormAsteroidsRemaining <= 0)
                    stormActive = false;
            }
        }
    }

    private void SpawnStormObstacle()
    {
        Vector2 spawnPos = cameraController.GetRandomSpawnPosition();
        Vector2 direction = (Vector2.zero - spawnPos).normalized;
        float angle = Random.Range(-45f, 45f);
        direction = Quaternion.Euler(0f, 0f, angle) * direction;

        Obstacle obstacle = poolSmall.Get();
        obstacle.Initialize(configSmall, spawnPos, direction,
                            currentSpeedMultiplier * 1.3f,
                            returnedObstacle => poolSmall.Release(returnedObstacle));
    }

    private void UpdateDifficulty()
    {
        if (elapsedTime < 30f)
        {
            currentSpawnRate = gameConfig.spawnRatePhase1;
            currentSpeedMultiplier = gameConfig.asteroidSpeedPhase1;
        }
        else if (elapsedTime < 60f)
        {
            currentSpawnRate = gameConfig.spawnRatePhase2;
            currentSpeedMultiplier = gameConfig.asteroidSpeedPhase2;
        }
        else if (elapsedTime < 90f)
        {
            currentSpawnRate = gameConfig.spawnRatePhase3;
            currentSpeedMultiplier = gameConfig.asteroidSpeedPhase3;
            TryTriggerStorm(asteroidCount: 8, interval: 60f);
        }
        else if (elapsedTime < 120f)
        {
            currentSpawnRate = gameConfig.spawnRatePhase4;
            currentSpeedMultiplier = gameConfig.asteroidSpeedPhase4;
            TryTriggerStorm(asteroidCount: 14, interval: 90f);
        }
        else
        {
            currentSpawnRate = gameConfig.spawnRatePhase5;
            currentSpeedMultiplier = gameConfig.asteroidSpeedPhase5;
            TryTriggerStorm(asteroidCount: 20, interval: 30f);
        }
    }

    private void TryTriggerStorm(int asteroidCount, float interval)
    {
        if (stormActive) return;
        float timeInPhase = elapsedTime % interval;
        if (timeInPhase < Time.deltaTime)
            StartStorm(asteroidCount);
    }

    private void StartStorm(int asteroidCount)
    {
        stormActive = true;
        stormAsteroidsRemaining = asteroidCount;
        stormTimer = 0f;

        EventBus.Publish(new AsteroidStormEvent
        {
            asteroidCount = asteroidCount,
            spawnInterval = StormSpawnInterval
        });

        Debug.Log($"[Storm] Asteroid Storm iniciado — {asteroidCount} asteroides");
    }

    private void SpawnObstacle()
    {
        // Elegir tipo de asteroide según probabilidad
        float roll = Random.value;
        ObstacleConfig config;
        GenericPool<Obstacle> pool;

        if (roll < SpawnChanceSmall)
        {
            config = configSmall;
            pool = poolSmall;
        }
        else if (roll < SpawnChanceSmall + SpawnChanceMedium)
        {
            config = configMedium;
            pool = poolMedium;
        }
        else
        {
            config = configBig;
            pool = poolBig;
        }

        // Posición de spawn fuera de pantalla
        Vector2 spawnPos = cameraController.GetRandomSpawnPosition();

        // Dirección hacia el área de juego con variación aleatoria
        Vector2 center = Vector2.zero;
        Vector2 direction = (center - spawnPos).normalized;

        // Añadir variación angular para que no todos vayan al centro exacto
        float angle = Random.Range(-30f, 30f);
        direction = Quaternion.Euler(0f, 0f, angle) * direction;

        // Obtener del pool e inicializar
        Obstacle obstacle = pool.Get();
        obstacle.Initialize(config, spawnPos, direction,
                            currentSpeedMultiplier,
                            returnedObstacle => pool.Release(returnedObstacle));
    }

    // ─── Eventos ──────────────────────────────────────────────────────

    private void OnRunStarted(RunStartedEvent e)
    {
        elapsedTime = 0f;
        spawnTimer = 0f;
        stormActive = false;
        stormAsteroidsRemaining = 0;
        stormTimer = 0f;
        isSpawning = true;
    }

    private void OnStateChanged(GameStateChangedEvent e)
    {
        // Detener spawn cuando el jugador muere
        if (e.newState == GameState.Death || e.newState == GameState.Upgrades)
            isSpawning = false;
    }
}
