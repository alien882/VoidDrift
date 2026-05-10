using UnityEngine;

/// <summary>
/// Gestiona el score, multiplicador y tiempo de supervivencia.
/// Publica cambios via EventBus para que la UI los muestre.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // Estado de la run actual
    private float elapsedTime;
    private float currentMultiplier;
    private float maxMultiplier;
    private int nearMissCount;
    private int currentScore;
    private bool isRunActive;

    // Acceso de solo lectura para la UI
    public int CurrentScore => currentScore;
    public float CurrentMultiplier => currentMultiplier;
    public float ElapsedTime => elapsedTime;
    public int HighScore => PlayerPrefs.GetInt("HighScore", 0);

    private GameConfig gameConfig;

    private void Start()
    {
        gameConfig = GameManager.Instance.Config;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Subscribe<NearMissEvent>(OnNearMiss);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Unsubscribe<NearMissEvent>(OnNearMiss);
    }

    private void Update()
    {
        if (!isRunActive) return;

        elapsedTime += Time.deltaTime;

        UpdateMultiplier();
        UpdateScore();
    }

    private void UpdateMultiplier()
    {
        // Multiplicador sube cada X segundos según GameConfig
        float newMultiplier = 1f + Mathf.Floor(elapsedTime / gameConfig.multiplierIncreaseInterval)
                                 * gameConfig.multiplierIncreaseAmount;

        if (!Mathf.Approximately(newMultiplier, currentMultiplier))
        {
            currentMultiplier = newMultiplier;
            maxMultiplier = Mathf.Max(maxMultiplier, currentMultiplier);

            EventBus.Publish(new ScoreChangedEvent
            {
                newScore = currentScore,
                multiplier = currentMultiplier
            });
        }
    }

    private void UpdateScore()
    {
        int newScore = Mathf.FloorToInt(elapsedTime * currentMultiplier);

        if (newScore != currentScore)
        {
            currentScore = newScore;
            EventBus.Publish(new ScoreChangedEvent
            {
                newScore = currentScore,
                multiplier = currentMultiplier
            });
        }
    }

    // ─── Eventos ──────────────────────────────────────────────────────

    private void OnRunStarted(RunStartedEvent e)
    {
        elapsedTime = 0f;
        currentMultiplier = 1f;
        maxMultiplier = 1f;
        nearMissCount = 0;
        currentScore = 0;
        isRunActive = true;
    }

    private void OnNearMiss(NearMissEvent e)
    {
        nearMissCount++;
        currentScore += e.bonusPoints;

        EventBus.Publish(new ScoreChangedEvent
        {
            newScore = currentScore,
            multiplier = currentMultiplier
        });
    }

    private void OnPlayerDied(PlayerDiedEvent e)
    {
        isRunActive = false;

        // Actualizar High Score
        if (currentScore > HighScore)
            PlayerPrefs.SetInt("HighScore", currentScore);

        // Enviar datos reales al GameManager — reemplaza los placeholders del Step 4
        GameManager.Instance.OnPlayerDied(
            finalScore: currentScore,
            survivalTime: elapsedTime,
            nearMissCount: nearMissCount,
            maxMultiplier: maxMultiplier
        );

        Debug.Log($"[ScoreManager] Run terminada → Score:{currentScore} " +
                  $"Tiempo:{elapsedTime:F1}s Mult:{maxMultiplier:F1} " +
                  $"NearMiss:{nearMissCount}");
    }

}
