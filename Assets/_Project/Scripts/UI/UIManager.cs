using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controlador central de la UI.
/// Muestra/oculta paneles según el estado del juego
/// y conecta botones con los sistemas de gameplay.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class UIManager : MonoBehaviour
{
    [SerializeField] private UpgradeManager upgradeManager;

    // Paneles
    private VisualElement hudPanel;
    private VisualElement gameOverPanel;
    private VisualElement upgradesPanel;

    // HUD
    private Label scoreLabel;
    private Label multiplierLabel;
    private Label highScoreLabel;
    private VisualElement borderWarning;

    // Game Over
    private Label finalScoreLabel;
    private Label survivalTimeLabel;
    private Label essenceEarnedLabel;

    // Upgrades
    private Label totalEssenceLabel;
    private VisualElement upgradesGrid;

    private DriftEssenceManager essenceManager;

    private void Awake()
    {
        UIDocument doc = GetComponent<UIDocument>();
        VisualElement root = doc.rootVisualElement;

        // Cachear referencias
        hudPanel = root.Q("HUDPanel");
        gameOverPanel = root.Q("GameOverPanel");
        upgradesPanel = root.Q("UpgradesPanel");

        scoreLabel = root.Q<Label>("ScoreLabel");
        multiplierLabel = root.Q<Label>("MultiplierLabel");
        highScoreLabel = root.Q<Label>("HighScoreLabel");
        borderWarning = root.Q("BorderWarning");

        finalScoreLabel = root.Q<Label>("FinalScoreLabel");
        survivalTimeLabel = root.Q<Label>("SurvivalTimeLabel");
        essenceEarnedLabel = root.Q<Label>("EssenceEarnedLabel");

        totalEssenceLabel = root.Q<Label>("TotalEssenceLabel");
        upgradesGrid = root.Q("UpgradesGrid");

        // Botones
        root.Q<Button>("RestartButton").clicked += OnRestartClicked;
        root.Q<Button>("UpgradesButton").clicked += OnUpgradesClicked;
        root.Q<Button>("ContinueButton").clicked += OnContinueClicked;

        ShowPanel(hudPanel);
    }

    private void Start()
    {
        essenceManager = FindFirstObjectByType<DriftEssenceManager>();
        UpdateHighScore();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);
        EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Subscribe<UpgradePurchasedEvent>(OnUpgradePurchased);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameStateChangedEvent>(OnStateChanged);
        EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Unsubscribe<UpgradePurchasedEvent>(OnUpgradePurchased);
    }

    // ─── Paneles ──────────────────────────────────────────────────────

    private void ShowPanel(VisualElement panelToShow)
    {
        hudPanel.RemoveFromClassList("panelVisible");
        gameOverPanel.RemoveFromClassList("panelVisible");
        upgradesPanel.RemoveFromClassList("panelVisible");

        panelToShow.AddToClassList("panelVisible");
    }

    // ─── Eventos de estado ────────────────────────────────────────────

    private void OnStateChanged(GameStateChangedEvent e)
    {
        switch (e.newState)
        {
            case GameState.Playing:
                ShowPanel(hudPanel);
                break;
            case GameState.Death:
                ShowPanel(gameOverPanel);
                break;
            case GameState.Upgrades:
                ShowPanel(upgradesPanel);
                BuildUpgradeCards();
                UpdateTotalEssence();
                break;
        }
    }

    // ─── HUD ──────────────────────────────────────────────────────────

    private void OnScoreChanged(ScoreChangedEvent e)
    {
        scoreLabel.text = e.newScore.ToString();
        multiplierLabel.text = $"x{e.multiplier:F1}";

        // Cambiar color del multiplicador según valor
        multiplierLabel.RemoveFromClassList("textCyan");
        multiplierLabel.RemoveFromClassList("textMagenta");
        multiplierLabel.RemoveFromClassList("textWhite");

        if (e.multiplier >= 3f) multiplierLabel.AddToClassList("textWhite");
        else if (e.multiplier >= 2f) multiplierLabel.AddToClassList("textMagenta");
        else multiplierLabel.AddToClassList("textCyan");
    }

    private void UpdateHighScore()
    {
        highScoreLabel.text = $"HI {PlayerPrefs.GetInt("HighScore", 0)}";
    }

    // Llamar desde PlayerController cuando se acerca a los bordes
    public void SetBorderWarning(bool active)
    {
        if (active) borderWarning.AddToClassList("borderWarningVisible");
        else borderWarning.RemoveFromClassList("borderWarningVisible");
    }

    // ─── Game Over ────────────────────────────────────────────────────

    private void OnPlayerDied(PlayerDiedEvent e)
    {
        // Solo actualizar si hay datos reales (segundo evento del GameManager)
        if (e.finalScore == 0 && e.survivalTime == 0f) return;

        finalScoreLabel.text = e.finalScore.ToString();
        survivalTimeLabel.text = $"{e.survivalTime:F1}s";
        essenceEarnedLabel.text = $"+{e.driftEssenceEarned:F0}";
        UpdateHighScore();
    }

    // ─── Upgrades ─────────────────────────────────────────────────────

    private void BuildUpgradeCards()
    {
        upgradesGrid.Clear();

        foreach (UpgradeData upgrade in upgradeManager.GetAllUpgrades())
        {
            VisualElement card = CreateUpgradeCard(upgrade);
            upgradesGrid.Add(card);
        }
    }

    private VisualElement CreateUpgradeCard(UpgradeData upgrade)
    {
        bool isMaxed = upgradeManager.IsMaxed(upgrade);
        int currentLevel = upgradeManager.GetLevel(upgrade);
        float nextCost = upgradeManager.GetNextCost(upgrade);
        bool canAfford = essenceManager.TotalEssence >= nextCost;

        VisualElement card = new();
        card.AddToClassList("upgradeCard");
        if (isMaxed) card.AddToClassList("upgradeCardMaxed");

        Label nameLabel = new() { text = upgrade.upgradeName };
        nameLabel.AddToClassList("upgradeName");

        Label effectLabel = new() { text = upgrade.effectDescription };
        effectLabel.AddToClassList("upgradeEffect");

        string levelText = isMaxed
            ? $"NIVEL MÁX ({upgrade.maxLevel})"
            : $"Nivel {currentLevel} / {upgrade.maxLevel}";
        Label levelLabel = new() { text = levelText };
        levelLabel.AddToClassList("upgradeLevel");

        Button buyBtn = new();
        buyBtn.AddToClassList("btnUpgrade");

        if (isMaxed)
        {
            buyBtn.text = "MÁXIMO";
            buyBtn.AddToClassList("btnUpgradeDisabled");
            buyBtn.SetEnabled(false);
        }
        else
        {
            buyBtn.text = $"{nextCost:F0} ✦";
            if (!canAfford)
                buyBtn.AddToClassList("btnUpgradeDisabled");

            buyBtn.clicked += () =>
            {
                if (upgradeManager.TryPurchase(upgrade))
                    BuildUpgradeCards(); // Reconstruir cards después de compra
            };
        }

        card.Add(nameLabel);
        card.Add(effectLabel);
        card.Add(levelLabel);
        card.Add(buyBtn);

        return card;
    }

    private void UpdateTotalEssence()
    {
        totalEssenceLabel.text = $"DRIFT ESSENCE: {essenceManager.TotalEssence:F0}";
    }

    private void OnUpgradePurchased(UpgradePurchasedEvent e)
    {
        UpdateTotalEssence();
    }

    // ─── Botones ──────────────────────────────────────────────────────

    private void OnRestartClicked() => GameManager.Instance.StartRun();
    private void OnUpgradesClicked() => GameManager.Instance.GoToUpgrades();
    private void OnContinueClicked() => GameManager.Instance.StartRun();
}
