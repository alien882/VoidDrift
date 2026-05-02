using UnityEngine;

/// <summary>
/// Controlador central del juego. Maneja la state machine y coordina
/// los sistemas a través del EventBus.
/// Singleton: acceso global via GameManager.Instance
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private GameConfig gameConfig;

    public GameConfig Config => gameConfig;
    public GameState CurrentState { get; private set; } = GameState.Playing;

    private void Awake()
    {
        // Patrón Singleton — solo puede existir una instancia
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre escenas (útil para futuras escenas de UI)
    }

    private void Start()
    {
        StartRun();
    }

    /// <summary>
    /// Inicia una nueva run. Llamar al empezar el juego y al reiniciar.
    /// </summary>
    public void StartRun()
    {
        EventBus.Clear(); // Limpia suscriptores de la run anterior
        ChangeState(GameState.Playing);
        EventBus.Publish(new RunStartedEvent());
    }

    /// <summary>
    /// Llamar cuando el jugador muere. Recibe los datos de la run.
    /// </summary>
    public void OnPlayerDied(int finalScore, float survivalTime, int nearMissCount, float maxMultiplier)
    {
        if (CurrentState != GameState.Playing) return; // Evita llamadas duplicadas

        float essence = gameConfig.CalculateDriftEssence(survivalTime, maxMultiplier, nearMissCount);

        ChangeState(GameState.Death);

        EventBus.Publish(new PlayerDiedEvent
        {
            finalScore = finalScore,
            survivalTime = survivalTime,
            driftEssenceEarned = essence
        });
    }

    /// <summary>
    /// Ir a la pantalla de upgrades desde Game Over.
    /// </summary>
    public void GoToUpgrades()
    {
        ChangeState(GameState.Upgrades);
    }

    /// <summary>
    /// Cambia el estado del juego y notifica a todos los suscriptores.
    /// </summary>
    private void ChangeState(GameState newState)
    {
        GameState previous = CurrentState;
        CurrentState = newState;

        EventBus.Publish(new GameStateChangedEvent
        {
            previousState = previous,
            newState = newState
        });

        Debug.Log($"[GameManager] Estado: {previous} → {newState}");
    }
}
