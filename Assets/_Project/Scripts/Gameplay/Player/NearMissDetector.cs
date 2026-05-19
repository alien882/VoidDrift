using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detecta Near Miss usando un trigger secundario más grande que el hitbox real.
/// Un Near Miss se confirma cuando un asteroide entra al trigger y sale
/// sin haber causado una colisión real con la nave.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class NearMissDetector : MonoBehaviour
{
    // Asteroides que están actualmente dentro del trigger
    private readonly HashSet<Collider2D> asteroidsInRange = new();

    private GameConfig gameConfig;
    private float lastNearMissTime = -999f;
    private bool isDead;

    private void Start()
    {
        gameConfig = GameManager.Instance.Config;

        // Configurar el radio del trigger según GameConfig
        CircleCollider2D trigger = GetComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = gameConfig.nearMissDistance;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);

        // Limpiar antes de que Unity dispare OnTriggerExit2D
        // al desactivar el collider
        asteroidsInRange.Clear();
        isDead = true;
    }

    // ─── Trigger ──────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (!other.CompareTag("Obstacle")) return;

        asteroidsInRange.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Obstacle")) return;

        // Si el asteroide sale del trigger y el jugador sigue vivo → Near Miss
        if (asteroidsInRange.Remove(other) && !isDead)
            TryRegisterNearMiss();
    }

    // ─── Lógica ───────────────────────────────────────────────────────

    private void TryRegisterNearMiss()
    {
        // Cooldown — evita spam si varios asteroides salen a la vez
        if (Time.time - lastNearMissTime < gameConfig.nearMissCooldown) return;

        lastNearMissTime = Time.time;

        int bonusPoints = gameConfig.nearMissBasePoints;

        EventBus.Publish(new NearMissEvent
        {
            bonusPoints = bonusPoints
        });

        Debug.Log($"[NearMiss] ¡Near Miss! +{bonusPoints} puntos");
    }

    // ─── Eventos ──────────────────────────────────────────────────────

    private void OnRunStarted(RunStartedEvent e)
    {
        isDead = false;
        asteroidsInRange.Clear();
        lastNearMissTime = -999f;
    }
}
