/// <summary>
/// Todos los eventos del juego definidos como structs.
/// Añadir nuevos eventos aquí a medida que se necesiten.
/// </summary>

// El estado del juego cambió
public struct GameStateChangedEvent
{
    public GameState previousState;
    public GameState newState;
}

// El jugador murió
public struct PlayerDiedEvent
{
    public int finalScore;
    public float survivalTime;
    public float driftEssenceEarned;
}

// El score cambió
public struct ScoreChangedEvent
{
    public int newScore;
    public float multiplier;
}

// Near Miss detectado
public struct NearMissEvent
{
    public int bonusPoints;
}

// El jugador compró un upgrade
public struct UpgradePurchasedEvent
{
    public string upgradeId;
    public int newLevel;
}

// Nueva run iniciada
public struct RunStartedEvent { }

// Las vidas del jugador cambiaron (upgrade Extra Life)
public struct LivesChangedEvent
{
    public int remainingLives;
}

// Evento de Asteroid Storm — ráfaga de asteroides pequeños
public struct AsteroidStormEvent
{
    public int asteroidCount;     // Cuántos asteroides spawnear en la ráfaga
    public float spawnInterval;   // Segundos entre cada spawn de la ráfaga
}