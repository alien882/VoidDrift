using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el movimiento de la nave del jugador y detecta colisiones.
/// Responsabilidad única: movimiento + muerte.
/// El score, la UI y los upgrades son responsabilidad de otros sistemas.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Datos del jugador")]
    [SerializeField] private PlayerData playerData;

    [Header("Referencias")]
    [SerializeField] private GameObject boosterFlame;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;

    [Header("Input")]
    [SerializeField] private InputAction thrustAction;
    [SerializeField] private InputAction lookAction;

    private bool isInvulnerable;
    private float ghostDashTimer;
    private float ghostDashCooldownTimer;
    private const float GhostDashDuration = 3f;
    private const float GhostDashCooldown = 60f;

    // Componentes
    private Rigidbody2D rb;
    private Camera mainCamera;
    private CameraController cameraController;

    // Estado interno
    private Vector2 thrustDirection;
    private bool isThrusting;
    private bool isDead;
    private int currentLives;

    // ─── Ciclo de vida ────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        cameraController = mainCamera.GetComponent<CameraController>();

        // Aplicar stats del PlayerData al Rigidbody
        rb.linearDamping = playerData.linearDrag;
        rb.angularDamping = playerData.angularDrag;

        // Aplicar escala de hitbox
        transform.localScale = Vector3.one * playerData.hitboxScale;
    }

    private void OnEnable()
    {
        thrustAction.Enable();
        lookAction.Enable();

        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
    }

    private void OnDisable()
    {
        thrustAction.Disable();
        lookAction.Disable();

        EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
    }

    private void Start()
    {
        currentLives = playerData.extraLives;
        isDead = false;
    }

    // ─── Update ───────────────────────────────────────────────────────

    private void Update()
    {
        if (isDead) return;

        isThrusting = thrustAction.IsPressed();

        if (isThrusting)
            RotateTowardInput();

        if (boosterFlame != null)
            boosterFlame.SetActive(isThrusting);

        // Añade esta línea
        if (audioManager != null)
            audioManager.PlayThrust(isThrusting);

        CheckBorderDeath();
        UpdateGhostDash();
    }

    private void UpdateGhostDash()
    {
        if (upgradeManager == null) return;

        UpgradeData ghostUpgrade = upgradeManager.GetAllUpgrades()
            .Find(u => u.upgradeType == UpgradeType.GhostDash);
        if (ghostUpgrade == null || upgradeManager.GetLevel(ghostUpgrade) == 0) return;

        ghostDashCooldownTimer += Time.deltaTime;

        if (!isInvulnerable && ghostDashCooldownTimer >= GhostDashCooldown)
        {
            ghostDashCooldownTimer = 0f;
            isInvulnerable = true;
            ghostDashTimer = 0f;
            Debug.Log("[GhostDash] Invulnerabilidad activada — 3s");
        }

        if (isInvulnerable)
        {
            ghostDashTimer += Time.deltaTime;
            if (ghostDashTimer >= GhostDashDuration)
            {
                isInvulnerable = false;
                Debug.Log("[GhostDash] Invulnerabilidad terminada");
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (isThrusting)
            rb.AddForce(thrustDirection * playerData.thrustForce, ForceMode2D.Force);

        ClampVelocity();
    }

    // ─── Movimiento ───────────────────────────────────────────────────

    private void RotateTowardInput()
    {
        Vector2 inputPosition = lookAction.ReadValue<Vector2>();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(inputPosition);
        worldPosition.z = 0f;

        thrustDirection = ((Vector2)worldPosition - (Vector2)transform.position).normalized;
        transform.up = thrustDirection;
    }

    private void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > playerData.maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * playerData.maxSpeed;
    }

    // ─── Muerte ───────────────────────────────────────────────────────

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isDead) return;
        if (!other.gameObject.CompareTag("Obstacle")) return;

        TakeDamage();
    }

    private void CheckBorderDeath()
    {
        if (cameraController == null) return;

        bool nearBorder = cameraController.IsOutOfBounds(
            transform.position, -2f); // Margen negativo = antes del borde

        // Actualizar warning visual
        if (uiManager != null)
            uiManager.SetBorderWarning(nearBorder);

        // Muerte al tocar el borde real
        if (cameraController.IsOutOfBounds(transform.position))
            TakeDamage();
    }

    private void TakeDamage()
    {
        if (isInvulnerable) return;

        if (currentLives > 0)
        {
            currentLives--;
            EventBus.Publish(new LivesChangedEvent { remainingLives = currentLives });
            return;
        }

        Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Efecto de explosión
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // Desactivar la nave — no destruir para no perder referencias
        gameObject.SetActive(false);

        // Publicar evento — ScoreManager se encarga del resto
        EventBus.Publish(new PlayerDiedEvent());
    }

    // ─── Eventos ──────────────────────────────────────────────────────

    private void OnRunStarted(RunStartedEvent e)
    {
        isDead = false;
        currentLives = playerData.extraLives;
        isInvulnerable = false;
        ghostDashTimer = 0f;
        ghostDashCooldownTimer = 0f;
        transform.position = Vector3.zero;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        gameObject.SetActive(true);
    }
}
